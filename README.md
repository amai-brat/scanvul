# scanvul

## TODO:
- [x] create command line agent installer with service registering on windows and linux (systemd)
- [x] CVE indexer microservice (opensearch)
- [ ] ФСТЭК (на фронте добавить блок с возможными уязвимостями без привязки к версии)
- [ ] main server api
  - [x] register agent
  - [x] version matching
    - [x] add BaseVersion (with segments split by \[.,-~:\]) that can be compared with all other version types 
  - [x] update CVEs snapshot
  - [ ] vulnerable package scanning jobs management
  - [ ] backend for frontend
  - [ ] tasks to agents
  - [ ] remove agent (task to remove)
  - [x] fix git 2.45.1 doesn't have [CVE](https://cti.wazuh.com/vulnerabilities/cves/CVE-2019-1003010). 
        Solution: vendor is jenkins with other version system, so I need to add feature to mark false positives   
  - [x] mark false positive vulnerabilities
- [ ] agent
  - [x] scrape packages on windows
  - [x] scrape packages on linux (alt linux)
  - [ ] task management (short pooling)
    - [ ] task to scan
    - [ ] task to upgrade package (via winget, chocolatey)
    - [ ] task to stop (remove)
- [ ] frontend
  - [x] agent's pc info
  - [x] vulnerable packages
  - [ ] severity viewer (query opensearch?)
  - [ ] task to upgrade package
  - [ ] i18n
  - [ ] mark false positive vulnerabilities