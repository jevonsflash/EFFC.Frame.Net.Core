@ echo off
:: ����ѹ��JS�ļ��ĸ�Ŀ¼���ű����Զ�������β��Һ�ѹ�����е�JS
SET JSFOLDER=%cd%
echo ���ڲ���JS�ļ�
chdir /d %JSFOLDER%
for /r . %%a in (*.min.js) do (
    @echo ����ɾ�� %%~pa%%~na.min.js ...
    del %%~fa

)
for /r . %%a in (*.js) do (
    @echo ����ѹ�� %%~pa%%~na.min.js ...
    uglifyjs %%~fa  -m -o %%~pa%%~na.min.js

)
echo ���!
::pause & exit