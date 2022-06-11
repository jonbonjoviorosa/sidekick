#Deployment Using Robocopy
#Admin
robocopy /S  C:\actions-runner\_work\Sidekick\Sidekick\Admin\bin\Release\net5.0\  C:\inetpub\wwwroot\sidekick\admin\ /e /mir /np 
#Api
robocopy /S  C:\actions-runner\_work\Sidekick\Sidekick\Api\bin\Release\net5.0\   C:\inetpub\wwwroot\sidekick\api\ /e /mir /np 
#Web
robocopy /S  C:\actions-runner\_work\Sidekick\Sidekick\Web\bin\Release\net5.0\  C:\inetpub\wwwroot\sidekick\web\ /e /mir /np 
#BookingPaymentProcess
robocopy /S  C:\actions-runner\_work\Sidekick\Sidekick\BookingPaymentProcessor\bin\Release\net5.0\ C:\inetpub\wwwroot\sidekick\BookingPaymentProcesso\/e /mir /np 

pause
