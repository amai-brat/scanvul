# scanvul

## TODO:
- [x] create command line agent installer with service registering on windows and linux (systemd)
- [x] CVE indexer microservice (opensearch)
- [ ] main server api
  - [x] register agent
  - [x] version matching
    - [x] add BaseVersion (with segments split by \[.,-~:\]) that can be compared with all other version types 
  - [x] update CVEs snapshot
  - [ ] vulnerable package scanning jobs management
  - [ ] backend for frontend
  - [ ] tasks to agents
  - [ ] remove agent (task to remove)
- [ ] agent
  - [x] scrape packages on windows
  - [x] scrape packages on linux (alt linux)
  - [ ] task management (long pooling)
    - [ ] task to scan
    - [ ] task to upgrade package (via winget, chocolatey)
    - [ ] task to stop (remove)
- [ ] frontend
  - [x] agent's pc info
  - [x] vulnerable packages
  - [ ] severity viewer (query opensearch?)
  - [ ] task to upgrade package