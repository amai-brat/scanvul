### Версия ПО у БДУ

- алгоритм добавления в opensearch:
  1. проверка на один из шаблонов (<версия> - это строка без пробелов):
      - ^от <версия>$
      - ^от <версия> до <версия>$
      - ^от <версия> по <версия>$
      - ^<версия> до <версия>$
      - ^<версия> по <версия>$
      - ^с <версия> до <версия>$
      - ^с <версия> по <версия>$
      - ^до <версия>$
      - ^от <версия> до <версия> включительно$
      - ^от <версия> по <версия> включительно$
      - ^<версия> до <версия> включительно$
      - ^<версия> по <версия> включительно$
      - ^с <версия> до <версия> включительно$
      - ^с <версия> по <версия> включительно$
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

### Базовая версия
Based on [this](https://github.com/microsoft/winget-cli/blob/master/doc/specs/%23980%20-%20Apps%20and%20Features%20entries%20version%20mapping.md?ysclid=mmcg8rxl32586549690).

Versions are parsed by:
1. Splitting the string based on the split characters: ```[',', '.', '~', '-', ':', ' ', '\t', '\n', '\r']```
2. Parsing a leading, positive integer from each split part
3. Saving any remaining, non-digits as a supplemental value
4. If a version part's value is 0 and it does not have supplemental value(non-digits), the version part is dropped(i.e. `1.0.0` will be parsed internally as version with only one part with value 1)

Versions are compared by:
- for each part in each version
  - if both sides have no more parts, return equal
  - else if one side has no more parts, it is less
  - else if integers not equal, return comparison of integers
  - else if only one side has a non-empty string part, it is less
  - else if string parts not equal, return comparison of strings

For example:
Version `1` is less than version `2`
Version `1.0.0` is less than version `2.0.0`
Version `0.0.1-alpha` is less than version `0.0.2-alpha`
Version `0.0.1-beta` is less than version `0.0.2-alpha`
Version `0.0.1-alpha` is less than version `0.0.1-beta`
Version `0.0.1-alpha` is less than version `0.0.1`
Version `13.9.8` is less than version `14.0`
Version `1.0` is equal to version `1.0.0`