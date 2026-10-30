"""Windows CurrentUser DPAPI upload signing. Never emits credentials or supplies them in argv.

init creates/reuses the stable private key outside Git; run executes only Gradle with
short-lived environment credentials. Back up the keystore AND DPAPI file securely;
DPAPI recovery additionally needs the original Windows user/profile recovery keys.
"""
import argparse
import ctypes
from ctypes import wintypes
import hashlib
import json
import os
from pathlib import Path
import secrets
import subprocess
import sys

JDK = Path('C:/Program Files/Unity/Hub/Editor/6000.3.19f1/Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK')
STORE = Path(os.environ.get('LOCALAPPDATA', '')) / 'Lowfire' / 'Signing'
ALIAS = 'lowfire-upload'

class Blob(ctypes.Structure):
    _fields_ = [('size', wintypes.DWORD), ('data', ctypes.POINTER(ctypes.c_ubyte))]

def crypt(raw, protect):
    crypt32 = ctypes.WinDLL('crypt32', use_last_error=True)
    kernel32 = ctypes.WinDLL('kernel32', use_last_error=True)
    buf = (ctypes.c_ubyte * len(raw)).from_buffer_copy(raw)
    source, output = Blob(len(raw), buf), Blob()
    if protect:
        fn = crypt32.CryptProtectData
        fn.argtypes = [ctypes.POINTER(Blob), wintypes.LPCWSTR, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p, wintypes.DWORD, ctypes.POINTER(Blob)]
        ok = fn(ctypes.byref(source), 'Lowfire stable upload signing', None, None, None, 1, ctypes.byref(output))
    else:
        fn = crypt32.CryptUnprotectData
        fn.argtypes = [ctypes.POINTER(Blob), ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p, ctypes.c_void_p, wintypes.DWORD, ctypes.POINTER(Blob)]
        ok = fn(ctypes.byref(source), None, None, None, None, 1, ctypes.byref(output))
    if not ok:
        raise RuntimeError('Windows CurrentUser DPAPI operation failed')
    kernel32.LocalFree.argtypes = [ctypes.c_void_p]
    try:
        return ctypes.string_at(output.data, output.size)
    finally:
        kernel32.LocalFree(output.data)

def restricted_directory():
    STORE.mkdir(parents=True, exist_ok=True)
    identity = subprocess.run(['whoami', '/user', '/fo', 'csv', '/nh'], capture_output=True, text=True, check=True).stdout.strip().split(',')[-1].strip('"')
    subprocess.run(['icacls', str(STORE), '/inheritance:r', '/grant:r', '*'+identity+':(OI)(CI)F', '*S-1-5-18:(OI)(CI)F'], capture_output=True, check=True)

def credentials():
    if not (STORE/'lowfire-upload.p12').is_file() or not (STORE/'upload-signing.dpapi').is_file():
        raise RuntimeError('Stable key is missing; run explicit init, never generate a disposable fallback')
    return json.loads(crypt((STORE/'upload-signing.dpapi').read_bytes(), False))

def environment(secret):
    result = os.environ.copy()
    result.update(LOWFIRE_UPLOAD_KEYSTORE=str(STORE/'lowfire-upload.p12'), LOWFIRE_UPLOAD_ALIAS=ALIAS, LOWFIRE_UPLOAD_PASSWORD=secret['password'])
    return result

def public_info(secret):
    p = subprocess.run([str(JDK/'bin/keytool.exe'), '-exportcert', '-alias', ALIAS, '-keystore', str(STORE/'lowfire-upload.p12'), '-storepass:env', 'LOWFIRE_UPLOAD_PASSWORD'], env=environment(secret), capture_output=True, creationflags=subprocess.CREATE_NO_WINDOW)
    if p.returncode:
        raise RuntimeError('Stored upload key/certificate could not be verified')
    der = p.stdout
    import base64
    encoded = base64.b64encode(der).decode()
    (STORE/'upload-certificate.pem').write_text('-----BEGIN CERTIFICATE-----\n'+'\n'.join(encoded[i:i+64] for i in range(0,len(encoded),64))+'\n-----END CERTIFICATE-----\n')
    info = {'alias': ALIAS, 'createdUTC': secret['createdUTC'], 'algorithm': 'RSA3072 / SHA256withRSA', 'validityDays':10000, 'certificateSHA1':hashlib.sha1(der).hexdigest().upper(), 'certificateSHA256':hashlib.sha256(der).hexdigest().upper(), 'passwordStorage':'Windows DPAPI CurrentUser', 'backupRequired': True}
    (STORE/'public-metadata.json').write_text(json.dumps(info, indent=2)+'\n')
    return info

def initialize():
    if os.name != 'nt':
        raise RuntimeError('Windows DPAPI required')
    restricted_directory()
    key, data = STORE/'lowfire-upload.p12', STORE/'upload-signing.dpapi'
    if key.exists() or data.exists():
        return public_info(credentials())
    from datetime import datetime, timezone
    secret = {'password':secrets.token_urlsafe(48), 'createdUTC':datetime.now(timezone.utc).isoformat()}
    # Persist encrypted recovery material before key generation; never a plaintext file.
    data.write_bytes(crypt(json.dumps(secret).encode(), True))
    p = subprocess.run([str(JDK/'bin/keytool.exe'), '-genkeypair', '-keystore', str(key), '-storetype', 'PKCS12', '-alias', ALIAS, '-keyalg', 'RSA', '-keysize', '3072', '-sigalg', 'SHA256withRSA', '-validity', '10000', '-dname', 'CN=Lowfire Upload,OU=Android Upload', '-storepass:env', 'LOWFIRE_UPLOAD_PASSWORD', '-keypass:env', 'LOWFIRE_UPLOAD_PASSWORD'], env=environment(secret), capture_output=True, creationflags=subprocess.CREATE_NO_WINDOW)
    if p.returncode:
        raise RuntimeError('Stable upload key generation failed; encrypted state preserved for recovery')
    return public_info(secret)

def gradle(project, log, tasks):
    secret = credentials()
    public_info(secret)
    project = Path(project).resolve()
    if not (project/'launcher/build.gradle').is_file():
        raise RuntimeError('Explicit Unity Gradle export required')
    script = (project/'launcher/build.gradle').read_text()
    if 'LOWFIRE_UPLOAD_PASSWORD' not in script or 'signingConfig signingConfigs.debug' in script:
        raise RuntimeError('Expected environment-only release signing configuration missing')
    unity_android = JDK.parent
    jars = list((unity_android/'Tools/gradle/lib').glob('gradle-launcher-*.jar'))
    if len(jars)!=1:
        raise RuntimeError('Pinned Unity Gradle launcher missing')
    cmd = [str(JDK/'bin/java.exe'), '-classpath', str(jars[0]), 'org.gradle.launcher.GradleMain', '--no-daemon', '--no-configuration-cache', '--console=plain', *tasks]
    with Path(log).open('w', encoding='utf-8') as output:
        process = subprocess.Popen(cmd, cwd=project, env=environment(secret), stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True, encoding='utf-8', errors='replace', creationflags=subprocess.CREATE_NO_WINDOW)
        for line in process.stdout:
            output.write(line.replace(secret['password'], '[REDACTED]'))
            output.flush()
        return process.wait()

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest='action', required=True)
    sub.add_parser('init'); sub.add_parser('public')
    run = sub.add_parser('run');run.add_argument('--project',required=True);run.add_argument('--log',required=True);run.add_argument('tasks',nargs='+')
    args = parser.parse_args()
    try:
        if args.action == 'init': print(json.dumps(initialize(), indent=2))
        elif args.action == 'public': print(json.dumps(public_info(credentials()), indent=2))
        else: sys.exit(gradle(args.project, args.log, args.tasks))
    except Exception as error:
        # Exceptions from external tools may contain raw environment-derived arguments.
        print('Signing operation failed: '+type(error).__name__, file=sys.stderr)
        sys.exit(1)
