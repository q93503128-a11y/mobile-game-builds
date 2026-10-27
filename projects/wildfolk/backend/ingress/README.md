# AdMob SSV durable ingress

Run Node22+, `npm ci` in backend and backend/ingress. Tests run from backend (`npm test`). This service uses the canonical backend modules, not a second wallet.

Container: from projects/wildfolk run `docker build -f backend/ingress/Dockerfile backend`. Only explicit source/package files are copied; local credentials, node_modules, player data and Unity assets are not copied. Docker/build/cloud deployment is not claimed by unit tests.

Deploy and review the exact setup in [ADS.md](../../docs/ADS.md). No endpoint, project or secret is provisioned by this repository. Starting with missing settings exits without printing values. Production additionally requires PRODUCTION_ADS_LIVE_ENABLED=true.

`GET /admob/ssv` verifies Google's exact encoded query and fixed Google keyserver, writes a durable CloudTask, then returns200. `POST /deliver` requires a Google-signed OIDC token for the configured worker service account, re-verifies the retained key/query and HMAC, then forwards to authenticated UGS. A provider callback is never sent to a Unity client. No request query/header/body is logged by the application. Configure infrastructure request-log redaction/exclusion before enabling SSV.
