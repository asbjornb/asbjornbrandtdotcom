# Rancher

To use rancher cli from powershell or command prompt one must first set environment variable: `RANCHER_CONFIG_DIR` to a location where configurations can be saved. Ex.: `C:\Users\asb\AppData\Local\Temp`

Common usage is:

* Check connection script in password manager.
  * The context key specifies which cluster to deploy to (staging, production local etc)
* Deploy with `rancher kubectl apply -f ${yamlPath}`

See bitwarden for up to date contexts and tokens.
