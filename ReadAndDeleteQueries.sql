SELECT * FROM [SimpleRabbitMQ].[dbo].[TestCommandHandler]
SELECT * FROM [SimpleRabbitMQ].[dbo].[TestEventHandler]
SELECT * FROM [SimpleRabbitMQ].[dbo].[OutboxRecord]

/*
delete FROM [SimpleRabbitMQ].[dbo].[TestCommandHandler]
delete FROM [SimpleRabbitMQ].[dbo].[TestEventHandler]
truncate table [SimpleRabbitMQ].[dbo].[OutboxRecord]
*/