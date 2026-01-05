# scanvul

## TODO:
- [x] create command line agent installer with service registering on windows and linux (systemd)
  - [x] exception when installing to computer where agent already exists
  - [x] use existing token when reinstalling 
- [x] CVE indexer microservice (opensearch)
- [ ] ФСТЭК (на фронте добавить блок с возможными уязвимостями без привязки к версии)
- [ ] main server api
  - [x] register agent
  - [x] version matching
    - [x] add BaseVersion (with segments split by \[.,-~:\]) that can be compared with all other version types 
  - [x] update CVEs snapshot
  - [x] vulnerable package scanning jobs management
  - [x] backend for frontend
  - [x] tasks to agents
  - [x] remove agent (task to remove)
  - [x] fix git 2.45.1 doesn't have [CVE](https://cti.wazuh.com/vulnerabilities/cves/CVE-2019-1003010). 
        Solution: vendor is jenkins with other version system, so I need to add feature to mark false positives   
  - [x] mark false positive vulnerabilities
  - [ ] admin notification
- [ ] agent
  - [x] scrape packages on windows
  - [x] scrape packages on linux (alt linux)
  - [x] task management (short pooling)
    - [x] task to scan
    - [x] task to upgrade package (via chocolatey)
    - [x] task to stop (remove)
  - [ ] conditional compilation for different OSes
- [ ] frontend
  - [x] agent's pc info
  - [x] vulnerable packages
  - [ ] severity viewer (query opensearch?)
    - [ ] block on main page with most important severities
  - [ ] task to upgrade package
  - [ ] i18n
  - [ ] mark false positive vulnerabilities