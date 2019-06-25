# PaloAltoUserId
A Windows service that reads NPS and DHCP logs from one or more Windows servers and reports username-to-IP address mappings to a Palo Alto Networks firewall.

Geared towards a school environment with a guest network that is forbidden to students.

## Quickstart
After building the code in Visual Studio, copy the SimpleServiceInstaller to a server:
```sh
md \\{SERVER_NAME}\c$\Program Files\PaloAltoUserId
cp -r SimpleServiceInstaller \\{SERVER_NAME}\c$\Program Files\PaloAltoUserId
```

Get an API key from your Palo Alto firewall:
```sh
& ./Get-PaloAltoApiKey.ps1 -PaloAltoMgmtAddress {PALOALTO_FQDN} -Username {PALOALTO_USERNAME} -Password {PALOALTO_PASSWORD}
```

Log into the server, tweak and install the registry settings, then install the service:
```sh
cd C:\Program Files\PaloAltoUserId
notepad Sample-PaloAltoUserId.reg
& ./Sample-PaloAltoUserId.reg
& ./Service-PaloAltoUserId.ps1 -Install -ProjectPath {ROOT_PATH_OF_VS_PROJECT}
```

## License
PaloAltoUserId is open source software [licensed as MIT](https://github.com/LookoutHill/PaloAltoUserId/blob/master/LICENSE).

