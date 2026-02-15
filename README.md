# scanvul

## TODO:
- [x] create command line agent installer with service registering on windows and linux (systemd)
  - [x] exception when installing to computer where agent already exists
  - [x] use existing token when reinstalling 
- [x] CVE indexer microservice (opensearch)
- [x] main server api
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
- [ ] agent
  - [x] scrape packages on windows
  - [x] scrape packages on linux (alt linux)
  - [x] task management (short pooling)
    - [x] task to scan
    - [x] task to upgrade package (via chocolatey)
    - [x] task to stop (remove)
  - [ ] conditional compilation for different OSes
- [x] frontend
  - [x] agent's pc info
  - [x] vulnerable packages
  - [x] severity viewer
  - [x] task to upgrade package
    - [x] search from package manager
  - [x] i18n
  - [x] mark false positive vulnerabilities
  - [x] refactor (extract components)
  - [x] toastify
    - [x] command creation
    - [x] searching package
- [x] ФСТЭК
  - [x] convert xml to json
  - [x] recurring job to export to opensearch
  - [x] entity for BDU vulnerable package
  - [x] endpoint like for CVE
  - [x] block on frontend with BDU
- [ ] find more appropriate package searching method [link](https://docs.opensearch.org/latest/query-dsl/term/index/)
- [ ] test
  - [ ] has vulnerability → update → no vulnerability
- [ ] deploy
  - [ ] docker compose
  - [ ] readme
- [ ] vulnerability report on organization every morning in pdf (s3) 
  - [ ] hangfire job to generate report
  - [ ] block on frontend on main page (accordion? make agent also as accordion?)
- [ ] change format of BDU documents' soft version info to like CVE documents'
  - алгоритм добавления в opensearch:
    1. проверка на один из шаблонов (<версия> - это строка без пробелов):
       - ^от <версия>$
       - ^от <версия> до <версия>$
       - ^от <версия> по <версия>$
       - ^до <версия>$
       - ^от <версия> до <версия> включительно$
       - ^от <версия> по <версия> включительно$
       - ^до <версия> включительно$
    2. добавить в vulnerable_software.soft.version_:
    ```json
    {
      "version": "<проверка на шаблон прошла> ? <ok> : <копирка vulnerable_software.soft.version>",
      "lt": "до|по",
      "lt_or_eq": "до|по влючительно",
      "gt_or_eq": "от (всегда включительно)"
    }
    ```
  - алгоритм сканирования ПО
    1. если vulnerable_software.soft.version_.version == "<ok>", то проверка по lt, lt_or_eq, gt_or_eq
    2. если проверка прошла, количество сегментов в проверяемом ПО и в <версия> может различаться на 1
    3. сравнение по lt, lt_or_eq, gt_or_eq
    4. при лжи хотя бы в одном из условий - пропуск (админ должен вручную нажимать "ложно-положительное")
