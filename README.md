
# Ветки

1. Репозиторий содержит ветки pa1, pa2 и т.д. с описанием заданий в файле README.md.
2. В вашем репозитории должны быть ветки с аналогичными названиями.
3. История выполнения задания должна вестись в рамках соответствующей ветки вашего репозитория.

# Как клонировать репозиторий

Рекомендуется:

1. Данный репозиторий установить как remote с именем "upstream"
2. Свой github-репозиторий установить как remote с именем "origin"

Через консольный клиент Git:

```bash
git clone https://github.com/tanyginvv/DISTRIBUTED-PROGRAMMING.git

git remote rename origin upstream

# Поменяйте your-name на свой логин github и создайте репозиторий ds-2026-labs
git remote add origin git@github.com:your-name/ds-2026-labs.git

git push origin main
```
## Для каждой новой лабы:
```bash
git fetch upstream              # подтянуть свежие шаблоны
git checkout main               # уйти в main
git checkout -b paN upstream/paN  # создать ветку из шаблона преподавателя
# ... выполняешь задание ...
git add .
git commit -m "PA N: сделано"
```