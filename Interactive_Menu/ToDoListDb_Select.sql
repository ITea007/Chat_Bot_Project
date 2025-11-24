-- GetAllByUserId - возвращает все задачи пользователя
SELECT * FROM "ToDoItem" WHERE "UserId" = '11111111-1111-1111-1111-111111111111';

-- GetActiveByUserId - возвращает активные задачи пользователя
SELECT * FROM "ToDoItem" WHERE "UserId" = '11111111-1111-1111-1111-111111111111' AND "State" = 0;

-- Get - возвращает задачу по ID
SELECT * FROM "ToDoItem" WHERE "Id" = '66666666-6666-6666-6666-666666666666';

-- ExistsByName - проверяет есть ли задача с таким именем у пользователя
SELECT COUNT(*) > 0 as "Exists" 
FROM "ToDoItem" 
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' AND "Name" = 'Написать отчет';

-- CountActive - возвращает количество активных задач у пользователя
SELECT COUNT(*) as "ActiveCount" 
FROM "ToDoItem" 
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' AND "State" = 0;

-- Find - возвращает задачи, удовлетворяющие предикату (пример: начинающиеся на "Написать")
SELECT * FROM "ToDoItem" 
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' 
AND "Name" LIKE 'Написать%';

-- Получение пользователя по TelegramUserId
SELECT * FROM "ToDoUser" WHERE "TelegramUserId" = 123456789;

-- Получение списков пользователя
SELECT * FROM "ToDoList" WHERE "UserId" = '11111111-1111-1111-1111-111111111111';

-- Получение задач по списку
SELECT * FROM "ToDoItem" 
WHERE "UserId" = '11111111-1111-1111-1111-111111111111' 
AND "ListId" = '33333333-3333-3333-3333-333333333333';