# PKI-back
Public key infrastructure backend API used for future of the project


## Possible environment variables
* XWS_PKI_HOST - ip where the database server is running (localhost for dev)
* XWS_PKI_PORT - port of the database server (for eg. postgres 5432)
* XWS_PKI_DATABASE - database file inside a server (for dev PIKDB)
* XWS_PKI_ADMINUSER - admin username for the api (if null, admin)
* XWS_PKI_ROOT_CERT_FOLDER - folder where to place the certificate, or to look for it (defaults to $USERPROFILE\.xws-cert\), here is where to find the symetric key for tls

## Necessary environment variables
* XWS_PKI_USERNAME - username for the database server
* XWS_PKI_PASSWORD - password for the database server
* XWS_PKI_ADMINPASS - admin password for the api

After the env variables have been set, restart rider/vs!

# Instructions

When the repo is cloned, first run the RootCertGenerator. After that, navigate to **%USERPROFILE%\.xws-cert\\** and double click apiCert.pfx. Install it for current user. When prompted for password, provide one that is in **XWS_PKI_ADMINPASS** env variable. After that select the _Trusted Root Certificate_ and proceed.

Now all should be done, when running the solution change from 'IIS Express' to just 'Api' and when the solution is started, the link will be https://localhost:44321/swagger

Run the ping method just so everything initializes.