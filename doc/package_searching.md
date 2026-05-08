3.5.2026  
текущие проблемы:
- не учитывается ОС / пакетный менеджер
- mozilla firefox не ищется в opensearch (нужен только firefox) при сканировании у винды
- visual-studio-code-bin искать без bin и в полях .platforms
- dotnet-runtime-8.0 имеет документы, где не указаны версии (чек defaultStatus) (вообще нужно изучить [это](https://github.com/CVEProject/cve-schema))

решение:
1. нормализовать строку
   - убрать -bin в конце
   - убрать "Inc.", "Corp.", "Corporation", "LLC", "GmbH"
   - убрать "(x64)", "(x86)", "64-bit", "en-US"
   - замена - на _ (кейс с visual_studio_code) или полностью убрать знаки препинания (учесть, что есть 7-zip, 7zip, visual_studio_code) (только для линукса?)
   - начинающиеся на python пакеты, проверять и по python3 и наоборот (python-pyasn1 <=> python3-pyasn1)
   - исключительные ситуации: убрать mozilla (для mozilla firefox) (substitions: mozilla firefox => firefox)
2. база данных с маппингом реестров в значения из CVE.product

CveRepository vs CveRepositoryV2 (+SearchTermSanitizerV2)
- 7zip: 14 | 28
- 7-zip: 23 | 28
- visual_studio_code: 52 | 52
- visual-studio-code: 0 | 52
- firefox: 3297 | 1000+
- mozilla firefox: 0 | 1000+
- chrome: 3818 | 1000+
- winrar: 27 | 28
- notepad++: 0 | 16
- powershell 7-x64: 0 | 23
- powershell: 23 | 23
- vlc media player: 0 | 0 (TODO: Sanitizer возвращает несколько вариантов)
- vlc_media_player: 112 | 112
- virtualbox: 406 | 415
- git: 86 | 87
- python: 191 | 316
- python-pyasn1: 1 | 2
- python3-pyasn1: 3 | 3
- pyasn1: 2 | 2
- tor: 101 | 101



