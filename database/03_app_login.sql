SET QUOTED_IDENTIFIER ON;
GO
/* Akun aplikasi dengan hak minimum (least privilege): hanya baca dan tulis data.
   Variabel sqlcmd APP_PASSWORD diberikan oleh setup.sh (-v APP_PASSWORD=...). */
USE master;
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'ct_app')
    CREATE LOGIN ct_app WITH PASSWORD = N'$(APP_PASSWORD)', CHECK_POLICY = OFF;
GO

USE CompanyTrack_DB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'ct_app')
    CREATE USER ct_app FOR LOGIN ct_app;
GO

ALTER ROLE db_datareader ADD MEMBER ct_app;
ALTER ROLE db_datawriter ADD MEMBER ct_app;
GO