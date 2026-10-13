"""Generate one original soft action cue using Python standard library."""
import math, struct, wave, argparse
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('output',type=Path);target=p.parse_args().output
target.mkdir(parents=True,exist_ok=True)
rate=22050;duration=.09
samples=[]
for i in range(int(rate*duration)):
    t=i/rate;envelope=min(1,t/.006)*(1-t/duration)**2
    signal=(math.sin(2*math.pi*(430*t-650*t*t))+.25*math.sin(2*math.pi*860*t))*.12*envelope
    samples.append(struct.pack('<h',int(signal*32767)))
with wave.open(str(target/'action.wav'),'wb') as f:
    f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(b''.join(samples))
