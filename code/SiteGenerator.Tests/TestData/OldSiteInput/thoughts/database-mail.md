# Database Mail

3 Steps to get job error mails:

* Enable and set up database mail
* Set up an operator
* Set notifications on individual jobs

[How To: Set Up Email Notifications for SQL Agent Jobs](http://thesqlnomad.com/set-up-email-notifications-for-sql-agent-jobs/)

References:

* <https://social.msdn.microsoft.com/Forums/en-US/39698a87-1400-4711-991c-1d9d4ea20f58/how-to-get-the-name-of-the-job-from-with-in-the-job-itself>
* <https://dba.stackexchange.com/questions/4734/how-can-i-have-sql-server-email-me-the-error-details-when-a-job-fails>

Code example:

```sql
USE [master]
GO 
--Turn on database mail feature:
sp_configure 'show advanced options',1 
GO 
reconfigure with override 
GO 
sp_configure 'Database Mail XPs',1 
GO 
reconfigure 
GO 

-- Set up mail profile AgentMailProfile
IF NOT EXISTS(SELECT * FROM msdb.dbo.sysmail_profile WHERE  name = 'AgentMailProfile')  
  BEGIN 
    EXECUTE msdb.dbo.sysmail_add_profile_sp 
      @profile_name = 'AgentMailProfile', 
      @description  = 'Mail profile for server agent to send out emails'; 
  END

-- Set up account sendinblue for smtp
  IF NOT EXISTS(SELECT * FROM msdb.dbo.sysmail_account WHERE  name = 'sendinblue') 
  BEGIN 
    EXECUTE msdb.dbo.sysmail_add_account_sp 
    @account_name            = 'sendinblue', 
    @email_address           = 'someemail', 
    @display_name            = 'somedisplayname', 
    @replyto_address         = 'someemail', 
    @description             = '', 
    @mailserver_name         = 'smtp-relay.sendinblue.com', 
    @mailserver_type         = 'SMTP', 
    @port                    = 'someport', 
    @username                = 'someemail', 
    @password                = 'somepassword',  
    @use_default_credentials =  0 , 
    @enable_ssl              =  0 ;
  END

-- Associate Account [sendinblue] to Profile [AgentMailProfile] 
IF NOT EXISTS(SELECT * 
              FROM msdb.dbo.sysmail_profileaccount pa 
                INNER JOIN msdb.dbo.sysmail_profile p ON pa.profile_id = p.profile_id 
                INNER JOIN msdb.dbo.sysmail_account a ON pa.account_id = a.account_id   
              WHERE p.name = 'AgentMailProfile' 
                AND a.name = 'sendinblue')  
  BEGIN 
    EXECUTE msdb.dbo.sysmail_add_profileaccount_sp 
      @profile_name = 'AgentMailProfile', 
      @account_name = 'sendinblue', 
      @sequence_number = 1 ; 
  END

-- Create operator to target with mails
USE [msdb]
GO

IF NOT EXISTS(select '1' from dbo.sysoperators where name = 'DataTeam')
EXEC msdb.dbo.sp_add_operator @name=N'DataTeam', 
  @enabled=1, 
  @weekday_pager_start_time=90000, 
  @weekday_pager_end_time=180000, 
  @saturday_pager_start_time=90000, 
  @saturday_pager_end_time=180000, 
  @sunday_pager_start_time=90000, 
  @sunday_pager_end_time=180000, 
  @pager_days=0, 
  @email_address=N'listofemailadresses', 
  @category_name=N'[Uncategorized]'
GO

```
