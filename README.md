# scanvul

## TODO:
- [x] create command line agent installer with service registering on windows and linux (systemd)
- [x] CVE indexer microservice (opensearch)
- [ ] main server api
  - [x] register agent
  - [x] version matching
    - [ ] add BaseVersion (with segments split by \[.,-~:\]) that can be compared with all other version types 
  - [x] update CVEs snapshot
  - [ ] vulnerable package scanning jobs management
  - [ ] backend for frontend
  - [ ] tasks to agents
- [ ] agent
  - [x] scrape packages on windows
  - [x] scrape packages on linux (alt linux)
  - [ ] task to scan
  - [ ] task to upgrade package (via winget, chocolatey)
- [ ] frontend
  - [ ] agent's pc info
  - [ ] vulnerable packages
  - [ ] task to upgrade package