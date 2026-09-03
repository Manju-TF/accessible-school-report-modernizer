libname erss2025 'C:\Users\DanielleTaylor\NALP\Research - Documents\ERSS\Class of 2025\Class of 2025 ERSS Submission Files';

*-----------------------------------------------------------------*
  this program, createschreptfile2.sas creates counts of various
  items
  by school, salary info by school, and puts it all together
 ----------------------------------------------------------------*;
options pagesize=63 linesize = 120 date number;

	
run;
proc format;
 value $time 'BGRAD' = 'Before Graduation'
             'AFTGRD' = 'After Graduation';
			
run;
proc format;
 value $source 
                 'OCI' = 'Career office recruitment program (e.g., OCI)'
				'JOBFRC' = 'Job fair or career conference'
               'JOBPST' = 'Career office job posting'
				'OTHER' = 'Other'
               'PRNSMJ' = 'Returned to or continued with pre-law school employer'
				'RFFRND' = 'Referral'
               'SELFPR' = 'Started own business/practice'
				'SLFINI' = 'Self-initiated contact/networking'
               'TEMPAG' = 'Temp agency or legal search consultant'
				'ONLINE' = 'Non-career office job posting'
                'OSCAR' = 'Clerkship application process or OSCAR';
RUN;

RUN;
proc format;
 value $jobcat  'LJD'='1-LJD'
               			'NLJD' = '2-NLJD'
               			'NLP' ='3-NLP'
               			'NLO'= '4-NLO'
               'ADVD'='6-ADVD'
			   'UDEF' = '7-UDEF'
               'USKW'='8-USKW'
			   'UNWK'='9-UNWK'
               'UNKN'='UNKN'
               'FULL' = 'Full-time'
               'PART' = 'Part-time'
                'WUNK' = '5-WUNK'
                ;
run;


data fornalprepts (drop = office_size   othersource  Field35b  field9b Field36  jobdesc);;
  set erss2025.erss2025;


    jobcat  = put (jobcat1, $jobcat.);
	if lfjob = 'ADMIN' then lfjob = 'YADMIN';
	if lfjob = 'OTHNL' then lfjob = 'ZOTHNL';
	if lfjob = 'STATTY' then lfjob = 'ATTYST';
	*if source in ('ONLINE','SOCI','TEMPAG') then source = 'OTHER';
	if jobreg = '0' then jobreg = 'X';
	if source = 'OTHER' then source = 'ZOTHER';
	if source = 'OCI' then source = 'AOCI';
	if emptype1 = 'JCLOGV' then emptype1 = 'JCTLOG';
	if emptype1 = 'JCINGV' then emptype1 = 'JCXIOG';
	if emptype1 in ('JCOTGV','JCUNGV','JC') then emptype1 = 'JCUGOV';
   * if sex in ('TW', 'W') then sex = 'F';
	*if sex = 'TM' then sex = 'M';
	if sex3 = 'W' then sex3 = 'F';
    if sex3 = 'X' then sex3 = 'N';
	if sex3 = 'ND' then sex3 = ' ';
	
		run;


proc sort data=fornalprepts;;
 by code;
 run;
proc freq data=fornalprepts noprint;
  table code/out=schrept1;


title Summary info by school;
title2 Class of 2025;
run;

 data schrept1;
  set schrept1;

  length newvar $15;
  length analvar $15;

  newvar = 'A';
  analvar = 'A';

format count comma6.0;

run;

title2 'file and count name is schrept1--COUNT OF total grads';
proc print data=schrept1 width = minimum ;
 sum count;
 
run;


proc sort data=fornalprepts;
 by code;
 run;
proc freq data=fornalprepts noprint;
  by code;
   where sex3 in ('M','F','N');
 table sex3/out=schrept2;


run;

 data schrept2;
  set schrept2;

  length newvar $15;
  length analvar $15;

  newvar = sex3;
  analvar = 'B';

format count comma6.0;

run;

title2 'file and count name is schrept2--COUNT by gender';
proc print data=schrept2  width = minimum;
 WHERE CODE LT '23200';
run;




proc freq data=fornalprepts noprint;
  by code;
   where minstat in ('NONMIN','MINOR');
 table minstat/out=schrept3;

run;

 data schrept3;
  set schrept3;

  length newvar $15;
  length analvar $15;

  newvar = minstat;
  analvar = 'C';

format count comma6.0;

run;

title2 'file and count name is schrept3--COUNT by MINOR status';
proc print data=schrept3 width =minimum ;
 WHERE CODE LT '23200';
run;


proc freq data=fornalprepts noprint;
  by code;
   where minstat in ('NONMIN','MINOR') and sex3 in ('F','M', 'N');
 table minstat*sex3/out=schrept4;


run;

 data schrept4;
  set schrept4;

  length newvar $15;
  length analvar $15;

  newvar = minstat||sex3;
  analvar = 'C1';

format count comma6.0;

run;
title2 'file and count name is schrept4--COUNT by gender and MINOR status';
proc print data=schrept4  width = minimum;
 WHERE CODE LT '23200';
run;



proc freq data=fornalprepts noprint;
  by code;
   where jobcat1 ne 'UNKN';
 table JOBCAT/out=schrept5;


run;

 data schrept5;
  set schrept5;

  length newvar $15;
  length analvar $15;

  newvar = JOBCAT;
  analvar = 'D';

format count comma6.0;

run;
title2 'file and count name is schrept5--COUNT of employment status';
proc print data=schrept5 width = minimum ;
 WHERE CODE LT '23200';
run;




proc freq data=fornalprepts noprint;
  by code;
   where jobcat1 ne 'UNKN';
 table JOBCAT/out=schrept6;


run;

 data schrept6;
  set schrept6;
   where JOBCAT not in ('7-USKW','8-UNWK');

  length newvar $15;
  length analvar $15;

  analvar = 'D1';

  if JOBCAT = '6-ADVD' then newvar = '6-ADVD';
  if JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO','5-WUNK')
    then newvar = 'EMPL';


format count comma6.0;

run;
 proc sort  data=schrept6 ;
   by code newvar;
   	run;
 proc means data=schrept6 sum noprint;
   by code newvar;
    var count percent;
   output out=schrept6 sum =;
 run;

 data schrept6 (drop= _freq_ _type_);
  set schrept6;
  analvar = 'D1';
run;

title2 'file and count name is schrept6--COUNT of degree and employed';
proc print data=schrept6 width = minimum ;
 WHERE CODE LT '23200';
run;



****ADDED NEW FOR 2002 and then modified in 2003--COUNT OF FT/PT JOBS****;
proc freq data=fornalprepts noprint;
  by code;
   where jobftpt ne ' ';
 table jobftpt*jobcat/out=schrept6a;
	format jobftpt $jobcat1.;


run;

 data schrept6a;
  set schrept6a;
   
  length newvar $15;
  length analvar $15;

  analvar = 'D3';
  newvar = compress (jobcat||jobftpt);
 
format count comma6.0;

run;



title2 'file and count name is schrept6a--COUNT of ft/pt jobs';
proc print data=schrept6a width = minimum ;
 WHERE CODE LT '23200';
run;


proc freq data=fornalprepts noprint;
  by code;
   where empgen ne ' ';
 table empgen/out=schrept7;


run;

 data schrept7;
  set schrept7;

  length newvar $15;
  length analvar $15;

  if empgen in ('ACAD','GOVT','CLERK','PUBINT') THEN NEWVAR = 'PUBLIC';
  IF empgen in ('BUS','FIRM') then newvar= 'PRIVATE';
  if empgen = 'EMPUNK' then delete;


format count comma6.0;

run;
proc sort data=schrept7;
 by code newvar;
 run;

 proc print data=schrept7 width = minimum ;
WHERE CODE LT '23200';
run;
 proc means data=schrept7 sum ; *noprint;
   by code newvar;
    var count percent;
   output out=schrept7 sum =;
 run;
 proc print data=schrept7 width = minimum ;
WHERE CODE LT '23200';
run;
 data schrept7 (drop= _freq_ _type_);
  set schrept7;
  analvar = 'D2';
run;

title2 'file and count name is schrept7--COUNT public/private emp';
proc print data=schrept7 width = minimum ;
WHERE CODE LT '23200';
run;


proc freq data=fornalprepts noprint;
  by code;
   where empgen ne ' ';
 table empgen/out=schrept8;


run;

 data schrept8;
  set schrept8;

  length newvar $15;
  length analvar $15;
  if empgen = 'EMPUNK' then empgen = 'ZEMPUN';
  newvar = empgen;
  analvar = 'E1';

format count comma6.0;

run;
title2 'file and count name is schrept8--COUNT by employer type';
proc print data=schrept8 width=minimum;
 WHERE CODE LT '23200';
run;

****job types within category***new for 2011;
proc freq data=fornalprepts noprint;
  by code;
   where empgen  = 'ACAD';
 table JOBCAT/out=schrept9a;


run;

 data schrept9a;
  set schrept9a;

  length newvar $15;
  length analvar $15;

  newvar = JOBCAT;
  analvar = 'E2';

format count comma6.0;

run;
title2 'file and count name is schrept9a--legal/nonlegal academic jobs';
proc print data=schrept9a width = minimum ;
 WHERE CODE LT '23200';
run;

proc freq data=fornalprepts noprint;
  by code;
   where empgen  = 'BUS';
 table JOBCAT/out=schrept9;


run;

 data schrept9;
  set schrept9;

  length newvar $15;
  length analvar $15;

  newvar = JOBCAT;
  analvar = 'E3';

format count comma6.0;

run;
title2 'file and count name is schrept9--legal/nonlegal biz jobs';
proc print data=schrept9 width = minimum ;
 WHERE CODE LT '23200';
run;

proc freq data=fornalprepts noprint;
  by code;
   where empgen  = 'FIRM';
 table JOBCAT/out=schrept9b;


run;

 data schrept9b;
  set schrept9b;

  length newvar $15;
  length analvar $15;

  newvar = JOBCAT;
  analvar = 'E4';

format count comma6.0;

run;
title2 'file and count name is schrept9--legal/nonlegal firm jobs';
proc print data=schrept9b width = minimum ;
 WHERE CODE LT '23200';
run;

proc freq data=fornalprepts noprint;
  by code;
   where empgen  = 'GOVT';
 table JOBCAT/out=schrept9c;


run;

 data schrept9c;
  set schrept9c;

  length newvar $15;
  length analvar $15;

  newvar = JOBCAT;
  analvar = 'E5';

format count comma6.0;

run;
title2 'file and count name is schrept9--legal/nonlegal govt jobs';
proc print data=schrept9c width = minimum ;
 WHERE CODE LT '23200';
run;
***added in for 2013--judicial clerkships by court;
proc freq data=fornalprepts noprint;
  by code;
   where empgen  = 'CLERK';
 table emptype1/out=schrept9cc;


run;

 data schrept9cc;
  set schrept9cc;

  length newvar $15;
  length analvar $15;

  newvar = emptype1;
  analvar = 'E55';

format count comma6.0;

run;
title2 'file and count name is schrept9cc--clerkship jobs jobs';
proc print data=schrept9cc width = minimum ;
 WHERE CODE LT '23200';
run;
proc freq data=fornalprepts noprint;
  by code;
   where empgen  = 'PUBINT';
 table JOBCAT/out=schrept9d;


run;

 data schrept9d;
  set schrept9d;

  length newvar $15;
  length analvar $15;

  newvar = JOBCAT;
  analvar = 'E6';

format count comma6.0;

run;
title2 'file and count name is schrept9d--legal/nonlegal pi jobs';
proc print data=schrept9d width = minimum ;
 WHERE CODE LT '23200';
run;


proc freq data=fornalprepts noprint;
  by code;
   where empgen  = 'FIRM';
 table firm1/out=schrept10;

run;

 data schrept10;
  set schrept10;

  length newvar $15;
  length analvar $15;

  analvar = 'FIRM';
  if firm1  = 'S' then newvar = 'SOLO';
   else if firm1 = '1' then newvar = 'LF1';
   else if firm1 = '2' then newvar = 'LF2';
   else if firm1 = '3' then newvar = 'LF3';
   else if firm1 = '4' then newvar = 'LF4';
   else if firm1 = '5' then newvar = 'LF5';
   else if firm1 = '6' then newvar = 'LF6';
   else if firm1 = '7' then newvar = 'LF7';
   else if firm1 = '8' then newvar = 'LF8';

format count comma6.0;

run;
title2 'file and count name is schrept10--law firm jobs by size';
proc print data=schrept10  width = minimum;
 WHERE CODE LT '23200';
run;

****try adding in the type of law firm job***;
proc freq data=fornalprepts noprint;
  by code;
   where empgen  = 'FIRM' and lfjob ne ' ';
 table lfjob/out=schrept10a;

run;

 data schrept10a;
  set schrept10a;

  length newvar $15;
  length analvar $15;

  analvar = 'FIRM2';
  newvar = lfjob;

format count comma6.0;

run;
title2 'file and count name is schrept10a--types of law firm jobs';
proc print data=schrept10a  width = minimum;
 WHERE CODE LT '23200';
run;

proc freq data=fornalprepts;* noprint;
  by code;
   where jobreg ge '0';
 table jobreg/out=schrept11;

run;

 data schrept11;
  set schrept11;

  length newvar $15;
  length analvar $15;

  analvar = 'JOBREG1';
   newvar = jobreg;

format count comma6.0;

run;
title2 'file and count name is schrept11--jobs by region';
proc print data=schrept11 width = minimum ;
 WHERE CODE LT '23200';
run;


proc freq data=fornalprepts noprint;
  by code;
   where jobreg ge '0';
 table locationflag/out=schrept12;
;
run;

 data schrept12;
  set schrept12;
 * if code = '90503' and locationflag =  'OUTOFSTATE' then count = 77;
  * if code = '90503' and locationflag =  '  ' then delete;

  length newvar $15;
  length analvar $15;

  analvar = 'JOBREG2';
   newvar = locationflag;

format count comma6.0;

run;
title2 'file and count name is schrept12--jobs instate/out';
proc print data=schrept12 width = minimum;
 WHERE code LT '23200';
run;


***for number of states with grads employed below, exclude forreign locations;

proc freq data=fornalprepts; *noprint;
  by code;
   where jobreg gt '0'and jobreg ne 'X';
 table jobst/out=schrept13;


;
run;

 proc freq data= schrept13;* noprint;
  by code;
   table jobst/out=schrept13;
   run;

proc means data=schrept13 sum ;*noprint;
 by code;
 var count;
  output out=schrept13 sum =;
  run;

 data schrept13 (drop= _freq_ _type_);
  set schrept13;
  length newvar $15;
  length analvar $15;
  analvar = 'JOBREG3';
  newvar = 'JOBREG3';
run;

title2 'file and count name is schrept13--# states where grads employed';
proc print data=schrept13 width = minimum ;
 WHERE CODE LT '23200';
run;


DATA schoolcounts2025;
 set schrept1 schrept2 schrept3
     schrept4  schrept5  schrept6  schrept6a
     schrept7 schrept8  schrept9 schrept9a schrept9b schrept9c schrept9cc schrept9d
     schrept10 schrept10a schrept11  schrept12 schrept13;

run;

proc sort data=schoolcounts2025;
 by code analvar newvar;
run;


*-----------------------------------------------------------------*
|now calculate the salary figures                               |
|                                          |
*-----------------------------------------------------------------*;
proc sort data=fornalprepts;
  by code sex3;
run;

proc univariate data=fornalprepts noprint;
  by code sex3;
  where sex3 in ('F','M', 'N');
  var salftperm;

 output out=schrept2a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;

run;


title ' Starting Salaries by School';

 data schrept2a;
  set schrept2a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'B';
  newvar = sex3;

  run;
title 'salary info by school';
title2 'file name is schrept2a--salaries by gender';

proc print data=schrept2a width = minimum;
 where code le '30000';
 run;


proc sort data=fornalprepts;
 by code minstat ;
 run;
proc univariate data=fornalprepts noprint;
  by code minstat;
  where minstat in ('NONMIN','MINOR');
  var salftperm;

 output out=schrept3a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School';

 data schrept3a;
  set schrept3a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'C';
  newvar = minstat;

  run;
title 'salary info by school';
title2 'file name is schrept3a--salaries by MINORority';

proc print data=schrept3a width = minimum;
 where code le '30000';
 run;


proc sort data=fornalprepts;
 by code minstat sex3 ;
 run;
proc univariate data=fornalprepts noprint;
  by code minstat sex3;
  where minstat in ('NONMIN','MINOR') and sex3 in ('F','M', 'N');
  var salftperm;

 output out=schrept4a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;

run;

title ' Starting Salaries by School';

 data schrept4a;
  set schrept4a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'C1';
  newvar = minstat||sex3;

  run;
title 'salary info by school';
title2 'file name is schrept4a--salaries by MINORority and gender';

proc print data=schrept4a width = minimum;
where code le '23300';
 run;


proc sort data=fornalprepts;
 by code JOBCAT ;
 run;
proc univariate data=fornalprepts noprint;
  by code JOBCAT;
  where JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO');
  var salftperm;

 output out=schrept5a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;

run;

title ' Starting Salaries by School';

 data schrept5a;
  set schrept5a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'D';
  newvar = JOBCAT;

  run;
title 'salary info by school';
title2 'file name is schrept5a--salaries by job type';

proc print data=schrept5a width = minimum;
 where code le '23300';
 run;


proc sort data=fornalprepts;
 by code  ;
 run;
proc univariate data=fornalprepts noprint;
  by code;

  var salftperm;

 output out=schrept6a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School';

 data schrept6a;
  set schrept6a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'D1';
  newvar = 'EMPL';

  run;
title 'salary info by school';
title2 'file name is schrept6a--salaries for all ft jobs';

proc print data=schrept6a width = minimum;
 where code le '30000';
 run;


proc sort data=fornalprepts;
 by code  ;
 run;
proc univariate data=fornalprepts noprint;
  by code;
     where empgen in ('BUS','FIRM');
  var salftperm;

 output out=schrept7a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School';

 data schrept7a;
  set schrept7a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'D2';
  newvar = 'PRIVATE';

  run;
title 'salary info by school';
title2 'file name is schrept7a--salaries for private jobs';

proc print data=schrept7a width = minimum;
 where code le '23300';
 run;


proc univariate data=fornalprepts noprint;
  by code;
     where empgen in ('ACAD','GOVT','CLERK','PUBINT');
  var salftperm;

 output out=schrept7b q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;

run;

title ' Starting Salaries by School';

 data schrept7b;
  set schrept7b;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'D2';
  newvar = 'PUBLIC';

  run;
title 'salary info by school';
title2 'file name is schrept7b--salaries for public jobs';

proc print data=schrept7b width = minimum;
 where code le '23300';
 run;



proc sort data=fornalprepts;
 by code empgen ;
 run;
proc univariate data=fornalprepts noprint;
  by code empgen;
  var salftperm;

 output out=schrept8a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;

run;


title ' Starting Salaries by School';

 data schrept8a;
  set schrept8a;
  where n ge 5;
  length analvar $15;
  length newvar $15;
    if empgen = 'EMPUNK' then empgen = 'ZEMPUN';
  analvar = 'E1';
  newvar = empgen;

  run;
title 'salary info by school';
title2 'file name is schrept8a--salaries by employer type ';

proc print data=schrept8a width = minimum;
 where code le '23300';
 run;


proc sort data=fornalprepts;
 by code JOBCAT ;
 run;
proc univariate data=fornalprepts noprint;
  by code JOBCAT;
   where empgen = 'BUS' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO');
  var salftperm;

 output out=schrept9a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School';

 data schrept9a;
  set schrept9a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'E3';
  newvar = JOBCAT;

  run;
title 'salary info by school';
title2 'file name is schrept9a--salaries biz jobs/legal non-legal';

proc print data=schrept9a width = minimum;
 where code le '23300';
 run;

 proc sort data=fornalprepts;
 by code JOBCAT ;
 run;
proc univariate data=fornalprepts noprint;
  by code JOBCAT;
   where empgen = 'ACAD' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO');
  var salftperm;

 output out=schrept9b q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School--academic jobs';

 data schrept9b;
  set schrept9b;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'E2';
  newvar = JOBCAT;

  run;
title 'salary info by school';
title2 'file name is schrept9b--salaries academic jobs/legal non-legal';

proc print data=schrept9b width = minimum;
 where code le '23300';
 run;

  proc sort data=fornalprepts;
 by code JOBCAT ;
 run;
proc univariate data=fornalprepts noprint;
  by code JOBCAT;
   where empgen = 'FIRM' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO');
  var salftperm;

 output out=schrept9c q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School--firm jobs';

 data schrept9c;
  set schrept9c;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'E4';
  newvar = JOBCAT;

  run;
title 'salary info by school';
title2 'file name is schrept9c--salaries firm jobs/legal non-legal';

proc print data=schrept9c width = minimum;
 where code le '23300';
 run;

 proc sort data=fornalprepts;
 by code JOBCAT ;
 run;
proc univariate data=fornalprepts noprint;
  by code JOBCAT;
   where empgen = 'GOVT' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO');
  var salftperm;

 output out=schrept9d q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School--govt jobs';

 data schrept9d;
  set schrept9d;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'E5';
  newvar = JOBCAT;

  run;
title 'salary info by school';
title2 'file name is schrept9d--salaries govt jobs/legal non-legal';

proc print data=schrept9d width = minimum;
 where code le '23300';
 run;

 ***for 2013 added in info on judicial clerkships;
  proc sort data=fornalprepts;
 by code emptype1 ;
 run;
proc univariate data=fornalprepts noprint;
  by code emptype1;
   where empgen = 'CLERK';
  var salftperm;

 output out=schrept9dd q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School--govt jobs';

 data schrept9dd;
  set schrept9dd;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'E55';
  newvar = emptype1;

  run;
title 'salary info by school';
title2 'file name is schrept9dd--salaries clerkships';

proc print data=schrept9dd width = minimum;
 where code le '23300';
 run;

 ***public interest;
  proc sort data=fornalprepts;
 by code JOBCAT ;
 run;
proc univariate data=fornalprepts noprint;
  by code JOBCAT;
   where empgen = 'PUBINT' and JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO');
  var salftperm;

 output out=schrept9e q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School--pi jobs';

 data schrept9e;
  set schrept9e;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'E6';
  newvar = JOBCAT;

  run;
title 'salary info by school';
title2 'file name is schrept9e--salaries pi jobs/legal non-legal';

proc print data=schrept9e width = minimum;
 where code le '23300';
 run;
proc sort data=fornalprepts;
 by code FIRM1 ;
 run;
proc univariate data=fornalprepts noprint;
  by code firm1;
   where empgen = 'FIRM';
  var salftperm;

 output out=schrept10a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;


title ' Starting Salaries by School';

 data schrept10a;
  set schrept10a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'FIRM';
  if firm1 = '1' then newvar = 'LF1';
   else if firm1 = '2' then newvar = 'LF2';
   else if firm1 = '3' then newvar = 'LF3';
   else if firm1 = '4' then newvar = 'LF4';
   else if firm1 = '5' then newvar = 'LF5';
   else if firm1 = '6' then newvar = 'LF6';
   else if firm1 = '7' then newvar = 'LF7';
   else if firm1 = '8' then newvar = 'LF8';


  run;
title 'salary info by school';
title2 'file name is schrept10a--salaries by firm size';

proc print data=schrept10a width = minimum;
 where code le '23000';
 run;

***salaries by lawfirm job type;
 proc sort data=fornalprepts;
 by code lfjob ;
 run;
proc univariate data=fornalprepts noprint;
  by code lfjob;
   where empgen = 'FIRM' and lfjob ne ' ';
  var salftperm;

 output out=schrept10b q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;

run;


title ' Starting Salaries by School';

 data schrept10b;
  set schrept10b;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'FIRM2';
  newvar = lfjob;


  run;
title 'salary info by school';
title2 'file name is schrept10b--salaries by type of firm job';

proc print data=schrept10b width = minimum;
 where code le '23000';
 run;


proc sort data=fornalprepts;
 by code jobreg ;
 run;
proc univariate data=fornalprepts noprint;
  by code jobreg;
   where jobreg ge '0';
  var salftperm;

 output out=schrept11a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;

title ' Starting Salaries by School';
run;
 data schrept11a;
  set schrept11a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'JOBREG1';
   newvar = jobreg;

  run;
title 'salary info by school';
title2 'file name is schrept11a--salaries by region';

proc print data=schrept11a width = minimum;
 where code le '23000';
 run;


proc sort data=fornalprepts;
 by code locationflag ;
 run;
proc univariate data=fornalprepts noprint;
  by code locationflag;
   where locationflag ne ' ';
  var salftperm;

 output out=schrept12a q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
title ' Starting Salaries by School';
   run;

data schrept12a;
  set schrept12a;
  where n ge 5;
  length analvar $15;
  length newvar $15;

  analvar = 'JOBREG2';
   newvar = locationflag;

  run;
title 'salary info by school';
title2 'file name is schrept12a--salaries by instate/outofstate';

proc print data=schrept12a width = minimum;
 where code le '23000';
 run;


  proc sort data=fornalprepts;
 by code JOBCAT ;
 run;
proc univariate data=fornalprepts noprint;
  by code JOBCAT;
   where JOBCAT in ('1-LJD','2-NLJD','3-NLP','4-NLO') ;
  var salftperm;

 output out=schrept14 q1 = pct25 median = median
        q3=pct75  mean=mean n=N;

  format pct25 median pct75  mean n comma7.0;
run;
data schrept14;
  set schrept14;
  where n ge 5;
  length analvar $15;
  length newvar $15;
  jobftpt = 'FULL';
  analvar = 'D3';
  newvar = compress (jobcat||jobftpt);
  
  run;

proc print data = schrept14;
  where code le '23000';
  run;
********************;
DATA schoolsalaries2025;
 set schrept2a schrept3a
     schrept4a schrept5a schrept6a
     schrept7a schrept7b
    schrept8a schrept9a schrept9b schrept9c schrept9d schrept9dd schrept9e    
    schrept10a schrept10b schrept11a schrept12a schrept14;
run;

proc sort data= schoolsalaries2025;
          by code analvar newvar;
 run;
proc sort data= schoolcounts2025;
          by code analvar newvar;
 run;

****now merge the counts and the salary info together****;

data erss2025.schreptsummary2025 (DROP = empgen firm1 JOBCAT
   jobreg locationflag minstat sex3  );
  merge schoolcounts2025 schoolsalaries2025;
  by code analvar newvar;
  
 run;

proc contents data=erss2025.schreptsummary2025;
title file of info for school report summary sheets;
title2 class of 2025;
run;

proc freq data=erss2025.schreptsummary2025;
  table analvar newvar;
run;


*create a second peice to the summary school report to report on 
law dchool funded jobs. set not set and long/short by employer type, sources, timing*;


****long term/short-term, ***;
proc sort data=fornalprepts;
 by code;
 run;
proc freq data=fornalprepts noprint;
  by code;
   where empgen ne ' ' and duration ne '';
 table duration /out = newreport1;
    
run;

  proc print data = newreport1;
   where code lt '23000';
   run;
proc transpose data = newreport1 out = newreport1 (drop = _name_   _label_);
  by code;
   var  count;
   id duration;
   run;
   proc print data = newreport1;
   where code lt '23000';
   run;

   proc freq data=fornalprepts noprint;
  by code;
   where empgen ne ' ' and duration ne '';
 table duration*empgen /out = newreport1a;
    
run;
proc print data = newreport1a;
 where code lt '23000';
run;
proc sort data = newreport1a;
  by code empgen;
  run;
proc transpose data = newreport1a out = newreport1a (drop = _name_   _label_);
  by code empgen;
   var  count;
   id duration;
    run;
   proc print data = newreport1a;
   where code lt '23000';
   run;


 
run;
data duration_final;
  set newreport1 newreport1a;
   length analvar $20;

  newvar = empgen;
  analvar = 'DURATION';
  run;
  proc sort data= duration_final;
   by code newvar;
   run;
  proc print data = duration_final;
   where code lt '23000';
   run;

***law school funded count****;
    proc freq data=fornalprepts noprint;
  by code ;
   where schoolfund ne ' ';
   table schoolfund/out = fund;
   run;
   proc print data = fund;
    where code lt '23000';
	run;

	data fund (drop = percent);
	 set fund;
	  where schoolfund in ('YES', 'Y');
	  rename count = perm;
	    length newvar $20;
        length analvar $20;
       	newvar = schoolfund;
		analvar = 'LAW SCHOOL FUNDED';
		run;
	 proc print data = fund;
    where code gt '90000';
	run;


****source of jobs****;

proc freq data=fornalprepts noprint;
  by code;
   where source ne ' ';
 table source/out=sourcetable;
    format source $source.;

run;

 data sourcetable;
  set sourcetable;

  length newvar $20;
  length analvar $20;

  newvar = source;
  analvar = 'SOURCE';

format count comma6.0;

run;

title2 'file and count name is sourcetable--source of jobs';
proc print data=sourcetable  width = minimum;

where code gt '90000';

run;


***timing of job offer****;

proc freq data=fornalprepts noprint;
  by code;
   where time1 ne ' ';
 table time1/out=timetable;
     format time1 $time.;

run;

 data timetable;
  set timetable;
  if time1 = 'AFTGRD' then time1 = 'ZAFTGRD';

  length newvar $20;
  length analvar $20;

  newvar = time1;
  analvar = 'TIME';

format count comma6.0;

run;
proc freq data = timetable;
run;

title2 'file and count name is timetable--time of job offers';
proc print data=timetable width = minimum ;
 WHERE CODE gt '90000';
run;

***search status of employed grads;
proc freq data=fornalprepts noprint;
  by code;
   where status ne ' ';
 table status/out=statustable;
     

run;

 data statustable;
  set statustable;

  length newvar $20;
  length analvar $20;

  newvar = status;
  analvar = 'ZSTATUS';

format count comma6.0;

run;

title2 'file and count name is STATUStable--SEARCH STATUS OF EMPLYED GRADS';
proc print data=STATUStable width = minimum ;
 WHERE CODE Lt '23000';
run;
 
DATA erss2025.schreptsummary2025_part2;

set sourcetable timetable statustable duration_final fund;
run;
proc freq DATA=erss2025.schreptsummary2025_part2;
  table analvar newvar;
  run;

 proc print DATA=erss2025.schreptsummary2025_part2;
  where code lt '23000';
 run;
