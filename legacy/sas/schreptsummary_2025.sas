libname erss2025 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS\Class of 2025\Class of 2025 ERSS Submission Files';
options source source2 mprint mlogic symbolgen;
options pagesize = 63 linesize = 110 nodate nonumber;
footnote ' ';
run; ;


PROC FORMAT;
     VALUE $jobcat 
       'LJD'  = '1-LJD'
       'NLJD' = '2-NLJD'
       'NLP'  = '3-NLP'
       'NLO'  = '4-NLO'
       'ADVD' = '6-ADVD'
	    'UDEF' = '7-UDEF'
       'USKW' = '8-USKW'
	   'UNWK' = '9-UNWK'
       'UNKN' = 'UNKN'
       'WUNK' = '5-WUNK'
      ;

	 
RUN;

PROC FORMAT;
     VALUE $recode
      'A'       = '   '
      'B'       = 'Gender Reported:'
      'C'       = 'Race Reported:'
      'D'       = 'Employment Status Known:'
      'D1'      = 'Total Employed or Enrolled in Graduate Studies:'
      'D2'      = 'Employment by Sector:'
      'D3'      = 'Full-time or Part-time Job Status:'
      'E1'      = 'Employment Categories:'
      'FIRM'    = 'Size of Law Firm (by # of Attorneys):'
      'FIRM2'   = 'Type of Law Firm Job:'
      'JOBREG1' = 'Jobs Taken by Region:'
      'C1'      = 'Gender & Race Reported:'
      'E'       = 'Employment by Sector:'
      'JOBREG2' = 'Location of Jobs:'
      'JOBREG3' = '# States and Territories with Employed Grads: '
      'SOURCE'  = 'Source of Job:'
      'TIME'    = 'Timing of Job Offer:'
      'E2'      = 'Education Jobs:'
	  'E3'      = 'Business Jobs:'
	  'E4'      = 'Private Practice Jobs:'
	  'E5'      = 'Government Jobs:'
	  'E55'     = 'Judicial Clerkships:'
	   'E6'     = 'Public Interest Jobs:'
      'ZSTATUS' = 'Search Status of Employed Grads:'
      'DURATION' = 'Duration of Jobs by Employer Type:'
      'LAW SCHOOL FUNDED' = 'Total Number of Jobs Reported as Funded by Law School:'; 
      ;
RUN;

PROC FORMAT;
     VALUE $subtotal
      'B'       = '   Subtotal'
      'C'       = '   Subtotal'
      'D'       = '   Subtotal'
      'D1'      = '   Subtotal'
      'D2'      = '   Subtotal'
      'D3'      = '   Subtotal'
      'E1'      = '   Subtotal'
      'FIRM'    = '   Subtotal'
      'FIRM2'   = '   Subtotal'
      'JOBREG1' = '   Subtotal'
      'C1'      = '   Subtotal'
      'E'       = '   Subtotal'
      'JOBREG2' = '   Subtotal'
      'JOBREG3' = '   Total #'
      'SOURCE'  = '   Subtotal'
      'TIME'    = '   Subtotal'
      'E2'      = '   Subtotal'
	  'E3'      = '   Subtotal'
	  'E4'      = '   Subtotal'
	  'E5'      = '   Subtotal'
	  'E55'     = '   Subtotal'
	  'E6'      = '   Subtotal'
	  'ZSTATUS' = '   Subtotal'
	  'DURATION' = '     Total Reported' 
      'LAW SCHOOL FUNDED' = '   Total Reported';
RUN;

PROC FORMAT;
     VALUE $newvar
      'F' = 'Women'
      'M' = 'Men'
	  'N' = 'Non-binary or Chose to Self-identify'
      'NONMIN' = 'White'
      'MINOR' = 'People of Color'
      'NONMINF' = 'White Women'
      'MINOR F' = 'Women of Color'
      'NONMINM' = 'White Men'
      'MINOR M' = 'Men of Color'
	  'NONMINN' = 'White Non-binary or Chose to Self-identify'
	  'MINOR N' = 'Non-binary or Chose to Self-identify People of Color'
      '1-LJD'='Bar Admission Required/ Anticipated'
      '2-NLJD' = 'JD Advantage'
      '3-NLP' ='Other Professional'
      '4-NLO'= 'Other Position'
      '5-WUNK' = 'Job Type Unknown'
      '9-UNWK' = 'Not Employed-Not Seeking'
      '6-ADVD' = 'Enrolled in Graduate Studies'
      '8-USKW' = 'Not Employed-Seeking'
	  '7-UDEF' = 'Employed-Start Date after March 16, 2026'
       'EMPL' = 'Employed'
      'A' = 'TOTAL '
      'ACAD' = 'Education'
      'BUS' = 'Business'
      'CLERK' = 'Judicial Clerkships'
      'GOVT' = 'Government'
      'FIRM' = 'Private Practice'
      'PUBINT' = 'Public Interest'
      'ZEMPUN' = 'Unknown Type'
	  'EMPUNK' = 'Unknown Type'
      'SOLO' = 'Solo Practitioner'
      'LF1' = '1-10'
      'LF2' = '11-25'
      'LF3' = '26-50'
      'LF4' = '51-100'
      'LF5' = '101-250'
      'LF6' = '251-500'
      'LF7' = '501+'
      'LF8' = 'Unknown Size'
	   'JCFDGV'  = 'Federal'
       'JCSTGV'  = 'State'
       'JCTLOG'  = 'Local'
	   'JCTRGV' = 'Tribal'
       'JCUGOV'  = 'Unknown'
		'JCINGV' = 'International'
		'JCXIOG' = 'International'	

      'X' = 'Non-US locations'
      '1' = 'New England'
      '2' = 'Mid-Atlantic'
      '3' = 'E North Central'
      '4' = 'W North Central'
      '5' = 'South Atlantic'
      '6' = 'E South Central'
      '7' = 'W South Central'
      '8' = 'Mountain'
      '9' = 'Pacific'
	  'T' = 'US Territories'
      'PRIVATE' = 'Private Sector'
      'PUBLIC' = 'Public Sector'
      'JOBREG3' = '   '
      'INSTATE' = 'In-State'
      'OUTOFSTATE' = 'Out of State'
      'FOREIGN' = 'Foreign'
    
      'PREBAR' = 'Before Bar Results'
      'AOCI'   = 'Career office recruitment program (e.g., OCI)'
      'JOBFRC' = 'Job fair or career conference'
      'JOBPST' = 'Career office job posting'
	   'OSCAR' = 'Clerkship application process or OSCAR'
      'ZOTHER'  = 'Other'
      'PRNSMJ' = 'Returned to or continued with pre-law school employer'
      'RFFRND' = 'Referral'
      'SELFPR' = 'Started own practice or business'
      'SLFINI' = 'Self-initiated contact/networking'
      'TEMPAG' = 'Temp agency'
      'FULL' = 'Full-time'
      'PART' = 'Part-time'
      'ONLINE' = 'Non-career office job posting'
      '1-LJDFULL'='Bar Admission Required/ Anticipated: Full-time'
      '2-NLJDFULL' = 'JD Advantage: Full-time '
      '3-NLPFULL' ='Other Professional: Full-time'
      '4-NLOFULL'= 'Other Position: Full-time'
      '5-WUNKFULL' = 'Job Type Unknown: Full-time'
      '1-LJDPART'='Bar Admission Required/ Anticipated: Part-time'
      '2-NLJDPART' = 'JD Advantage: Part-time '
      '3-NLPPART' ='Other Professional: Part-time'
      '4-NLOPART'= 'Other Position: Part-time'
      '5-WUNKPART' = 'Job Type Unknown: Part-time'
      'ATTY' = 'Associate/Entry-level Attorney'
      'LCLERK' = 'Law Clerk'
      'PARA' = 'Paralegal'
	  'PATAGT' = 'Patent Agent'
      'YADMIN' = 'Manager/administrator'
	  'ATTYST' = 'Staff Attorney'
	  'ZOTHNL' = 'Other Non-attorney Position'
      'SET' = 'Not seeking a different job'
      'NOTSET' = 'Seeking a different job'
      'Z-Total Reporte' = 'Total Number Reported'
      'YES' = ' '
       'BGRAD' = 'Before graduation'
      'ZAFTGR' = 'After graduation';
    
      ;
RUN;


 

/*************************************************************************************************/
/* Set up macro variables to drive report and not hard code counts **/
/** see email from SAS on 6/26/2019 about fixing macro issues***/
/*************************************************************************************************/

DATA _NULL_;
     SET erss2025.schreptsummary2025(WHERE=(ANALVAR EQ "A"));

	 CALL SYMPUT(COMPRESS("Ct_" || Code), TRIM(LEFT(PUT(Count, 4.))));
RUN;

options pagesize= 63 linesize=100 nodate nonumber MLOGIC SYMBOLGEN; *mprint nonotes;
 


  /****version for creating one long pdf for original mailing****/

/***ods pdf file = 'C:\myDocuments\schoolsummaries2017.pdf****/



%MACRO SCHRPTS(CODE, NAME, JOBST, ST);
 

ODS ESCAPECHAR = '^';
 
/**********************************/
/**** First page of the report ****/
/**********************************/
ods proclabel='Page 1';
PROC REPORT DATA=erss2025.schreptsummary2025 NOWD HEADSKIP HEADLINE SPLIT='*'
     STYLE(COLUMN)=[font_size = 8pt  cellwidth = 0.75 in ]
     STYLE(HEADER LINES) = [font_size = 9pt]
	STYLE = [posttext  = "^n Table prepared by NALP, July 2026
      ^n NALP Summary Report data may vary slightly from the school-specific data published by the ABA Council because NALP's quality control process can result in changes which may not be reflected in ABA Council data. For more on this, see www.nalp.org/erssinfo."
		  font_size = 8pt];
	/**use this version of the footnote if need to reprint a report - it will autogenerate today's date**/
	 /*88STYLE = [posttext  = "^n Table prepared by NALP, August 2020***/
     /***         ^n Table reprinted by NALP on %sysfunc(left(%qsysfunc(today(),weekdate.)))" ***/
       /***      font_size = 8pt]; ****/
	   
  

     TITLE1 "&NAME";
     TITLE2 "Class of 2025 Summary Report";
   
     WHERE CODE = "&CODE" AND ANALVAR in ('B','C','C1','D') ;

	 COLUMN AnalVar NewVar Count Percent
           ('Full-time Long-term Salaries' N Pct25 Median Pct75 Mean);

     DEFINE Analvar / GROUP NOPRINT;
     DEFINE Newvar  / ORDER = INTERNAL GROUP '  ' FORMAT = $NEWVAR. STYLE = [CELLWIDTH = 1.75 IN JUST = LEFT];
     DEFINE Count   / SUM     'Number*Reported'   FORMAT = COMMA6.0 ;
     DEFINE Percent / SUM     '% of*Reported'     FORMAT = 6.1;
     DEFINE N       / DISPLAY '# with*Salary'     FORMAT = COMMA6.0;
     DEFINE Pct25   / DISPLAY '25th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Median  / DISPLAY 'Median'            FORMAT = COMMA8.0;
     DEFINE Pct75   / DISPLAY '75th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Mean    / DISPLAY 'Mean'              FORMAT = COMMA8.0;

     COMPUTE BEFORE / STYLE = [font_weight = bold  font_size = 8pt];;
       LINE@2 ' ';
       LINE@2 "Total Reported = &&Ct_&Code.";
     ENDCOMP;

     COMPUTE BEFORE ANALVAR / STYLE = [font_weight = bold  font_size = 8pt];;
       LINE @2 ' ';
       LINE @2 Analvar $recode.;
     ENDCOMP;
  
     COMPUTE AFTER ANALVAR / STYLE = [font_weight = bold  font_size = 8pt  asis = on];
       LINE@10 Analvar     $subtotal.       
           +47 Count.sum   comma6.0
           +17 Percent.sum 6.1;
     ENDCOMP;

     COMPUTE AFTER;
    LINE@0 'Note: Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. The non-binary or chose to self-identify category also includes graduates who selected multiple gender identities. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.';
     ENDCOMP;

     FOOTNOTE1 ' ';
     FOOTNOTE2 ' ';
RUN;

/***********************************/
/**** Second page of the report ****/
/***********************************/
ods proclabel='Page 2';
PROC REPORT DATA=erss2025.schreptsummary2025 NOWD HEADSKIP HEADLINE SPLIT='*'
     STYLE(COLUMN)=[font_size = 8pt  cellwidth = 0.75 in ]
     STYLE(HEADER LINES) = [font_size = 9pt]
		STYLE = [posttext  = "^n Table prepared by NALP, July 2026
      ^n NALP Summary Report data may vary slightly from the school-specific data published by the ABA Council because NALP's quality control process can result in changes which may not be reflected in ABA Council data. For more on this, see www.nalp.org/erssinfo."
			  font_size = 8pt];
	/*** STYLE = [posttext  = "^n Table prepared by NALP, August 2020 ***/
   /***          ^n Table reprinted by NALP on %sysfunc(left(%qsysfunc(today(),weekdate.))) " ***/
     /**        font_size = 8pt]; ***/

	

     TITLE1 "&NAME";
     TITLE2 "Class of 2025 Summary Report - Page 2";
   
     WHERE CODE = "&CODE" AND ANALVAR GE 'D1' AND ANALVAR LT 'E2';
	 COLUMN AnalVar NewVar Count Percent
           ('Full-time Long-term Salaries' N Pct25 Median Pct75 Mean);

     DEFINE Analvar / GROUP NOPRINT;
     DEFINE Newvar  / ORDER = INTERNAL GROUP '  ' FORMAT = $NEWVAR. STYLE = [CELLWIDTH = 1.75 IN JUST = LEFT];
     DEFINE Count   / SUM     'Number*Reported'   FORMAT = COMMA6.0 ;
     DEFINE Percent / SUM     '% of*Reported'     FORMAT = 6.1;
     DEFINE N       / DISPLAY '# with*Salary'     FORMAT = COMMA6.0;
     DEFINE Pct25   / DISPLAY '25th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Median  / DISPLAY 'Median'            FORMAT = COMMA8.0;
     DEFINE Pct75   / DISPLAY '75th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Mean    / DISPLAY 'Mean'              FORMAT = COMMA8.0;

     COMPUTE BEFORE ANALVAR / STYLE = [font_weight = bold  font_size = 8pt];;
       LINE @2 ' ';
       LINE @2 Analvar $recode.;
     ENDCOMP;

     COMPUTE AFTER ANALVAR / STYLE = [font_weight = bold  font_size = 8pt  asis = on];
       LINE@10 Analvar     $subtotal.       
           +47 Count.sum   comma6.0
           +17 Percent.sum 6.1;
     ENDCOMP;

     COMPUTE AFTER;
 LINE@0 'Note: Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. Private sector includes jobs in law firms and business. All other jobs are considered public sector. Employment by sector does not include graduates for whom employer type was not reported. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.';

     ENDCOMP;

     FOOTNOTE1 ' ';
     FOOTNOTE2 ' ';
RUN;

/**********************************/
/**** Third page of the report ****/
/**********************************/
ods proclabel='Page 3';
PROC REPORT DATA=erss2025.schreptsummary2025 NOWD HEADSKIP HEADLINE SPLIT='*'
     STYLE(COLUMN)=[font_size = 8pt  cellwidth = 0.75 in ]
     STYLE(HEADER LINES) = [font_size = 9pt]
	STYLE = [posttext  = "^n Table prepared by NALP, July 2026
      ^n NALP Summary Report data may vary slightly from the school-specific data published by the ABA Council because NALP's quality control process can result in changes which may not be reflected in ABA Council data. For more on this, see www.nalp.org/erssinfo."
		  font_size = 8pt];
	
	/*** STYLE = [posttext  = "^n Table prepared by NALP, August 2020 ***/
   /***          ^n Table reprinted by NALP on %sysfunc(left(%qsysfunc(today(),weekdate.))) " ***/
     /**        font_size = 8pt]; ***/

   
     TITLE1 "&NAME";
     TITLE2 "Class of 2025 Summary Report - Page 3";
   
     WHERE CODE = "&CODE" AND ANALVAR in ('E2','E3','E4','E5','E55') ;

	 COLUMN AnalVar NewVar Count Percent
           ('Full-time Long-term Salaries' N Pct25 Median Pct75 Mean);

     DEFINE Analvar / GROUP NOPRINT;
     DEFINE Newvar  / ORDER = INTERNAL GROUP '  ' FORMAT = $NEWVAR. STYLE = [CELLWIDTH = 1.75 IN JUST = LEFT];
     DEFINE Count   / SUM     'Number*Reported'   FORMAT = COMMA6.0 ;
     DEFINE Percent / SUM     '% of*Reported'     FORMAT = 6.1;
     DEFINE N       / DISPLAY '# with*Salary'     FORMAT = COMMA6.0;
     DEFINE Pct25   / DISPLAY '25th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Median  / DISPLAY 'Median'            FORMAT = COMMA8.0;
     DEFINE Pct75   / DISPLAY '75th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Mean    / DISPLAY 'Mean'              FORMAT = COMMA8.0;

     COMPUTE BEFORE ANALVAR / STYLE = [font_weight = bold  font_size = 8pt];;
       LINE @2 ' ';
       LINE @2 Analvar $recode.;
     ENDCOMP;

     COMPUTE AFTER ANALVAR / STYLE = [font_weight = bold  font_size = 8pt  asis = on];
       LINE@10 Analvar     $subtotal.       
           +47 Count.sum   comma6.0
           +17 Percent.sum 6.1;
     ENDCOMP;

     COMPUTE AFTER;
       LINE@0 'Note: Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.';
        ENDCOMP;

     FOOTNOTE1 ' ';
     FOOTNOTE2 ' ';
RUN;

/************************************************/
 /***Fourth page of the report***/
/************************************************/
ods proclabel='Page 4';

PROC REPORT DATA=erss2025.schreptsummary2025 NOWD HEADSKIP HEADLINE SPLIT='*'
     STYLE(COLUMN)=[font_size = 8pt  cellwidth = 0.75 in ]
     STYLE(HEADER LINES) = [font_size = 9pt]
	STYLE = [posttext  = "^n Table prepared by NALP, July 2026
      ^n NALP Summary Report data may vary slightly from the school-specific data published by the ABA Council because NALP's quality control process can result in changes which may not be reflected in ABA Council data. For more on this, see www.nalp.org/erssinfo."
           font_size = 8pt];
	  
	/*** STYLE = [posttext  = "^n Table prepared by NALP, August 2020 ***/
   /***          ^n Table reprinted by NALP on %sysfunc(left(%qsysfunc(today(),weekdate.))) " ***/
     /**        font_size = 8pt]; ***/
	
   
     TITLE1 "&NAME";
     TITLE2 "Class of 2025 Summary Report - Page 4";
   
     WHERE CODE = "&CODE" AND ANALVAR IN ('E6','FIRM', 'FIRM2') ;

	 COLUMN AnalVar NewVar Count Percent
           ('Full-time Long-term Salaries' N Pct25 Median Pct75 Mean);

     DEFINE Analvar / GROUP NOPRINT;
     DEFINE Newvar  / ORDER = INTERNAL GROUP '  ' FORMAT = $NEWVAR. STYLE = [CELLWIDTH = 1.75 IN JUST = LEFT];
     DEFINE Count   / SUM     'Number*Reported'   FORMAT = COMMA6.0 ;
     DEFINE Percent / SUM     '% of*Reported'     FORMAT = 6.1;
     DEFINE N       / DISPLAY '# with*Salary'     FORMAT = COMMA6.0;
     DEFINE Pct25   / DISPLAY '25th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Median  / DISPLAY 'Median'            FORMAT = COMMA8.0;
     DEFINE Pct75   / DISPLAY '75th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Mean    / DISPLAY 'Mean'              FORMAT = COMMA8.0;

     COMPUTE BEFORE ANALVAR / STYLE = [font_weight = bold  font_size = 8pt];;
       LINE @2 ' ';
       LINE @2 Analvar $recode.;
     ENDCOMP;

     COMPUTE AFTER ANALVAR / STYLE = [font_weight = bold  font_size = 8pt  asis = on];
       LINE@10 Analvar     $subtotal.       
           +47 Count.sum   comma6.0
           +17 Percent.sum 6.1;
     ENDCOMP;

     COMPUTE AFTER;
    LINE@0 'Note: Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.';
     ENDCOMP;

     FOOTNOTE1 ' ';
     FOOTNOTE2 ' ';
RUN;
/***************************************/
/*** Fifth page of the report****/
/***************************************/
ods proclabel='Page 5';

PROC REPORT DATA=erss2025.schreptsummary2025 NOWD HEADSKIP HEADLINE SPLIT='*'
     STYLE(COLUMN)=[font_size = 8pt  cellwidth = 0.75 in ]
     STYLE(HEADER LINES) = [font_size = 9pt]
	STYLE = [posttext  = "^n Table prepared by NALP, July 2026
      ^n NALP Summary Report data may vary slightly from the school-specific data published by the ABA Council because NALP's quality control process can result in changes which may not be reflected in ABA Council data. For more on this, see www.nalp.org/erssinfo."
		  font_size = 8pt];
		/*** STYLE = [posttext  = "^n Table prepared by NALP, August 2020 ***/
   /***          ^n Table reprinted by NALP on %sysfunc(left(%qsysfunc(today(),weekdate.))) " ***/
     /**        font_size = 8pt]; ***/
   
     TITLE1 "&NAME";
     TITLE2 "Class of 2025 Summary Report - Page 5";
   
     WHERE CODE = "&CODE" AND ANALVAR IN ('JOBREG1','JOBREG2','JOBREG3') ;

	 COLUMN AnalVar NewVar Count Percent
           ('Full-time Long-term Salaries' N Pct25 Median Pct75 Mean);

     DEFINE Analvar / GROUP NOPRINT;
     DEFINE Newvar  / ORDER = INTERNAL GROUP '  ' FORMAT = $NEWVAR. STYLE = [CELLWIDTH = 1.75 IN JUST = LEFT];
     DEFINE Count   / SUM     'Number*Reported'   FORMAT = COMMA6.0 ;
     DEFINE Percent / SUM     '% of*Reported'     FORMAT = 6.1;
     DEFINE N       / DISPLAY '# with*Salary'     FORMAT = COMMA6.0;
     DEFINE Pct25   / DISPLAY '25th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Median  / DISPLAY 'Median'            FORMAT = COMMA8.0;
     DEFINE Pct75   / DISPLAY '75th*Percentile'   FORMAT = COMMA8.0;
     DEFINE Mean    / DISPLAY 'Mean'              FORMAT = COMMA8.0;

     COMPUTE BEFORE ANALVAR / STYLE = [font_weight = bold  font_size = 8pt];;
       LINE @2 ' ';
       LINE @2 Analvar $recode.;
     ENDCOMP;

     COMPUTE AFTER ANALVAR / STYLE = [font_weight = bold  font_size = 8pt  asis = on];
       LINE@10 Analvar     $subtotal.       
           +47 Count.sum   comma6.0
           +17 Percent.sum 6.1;
     ENDCOMP;

     COMPUTE AFTER;
  	LINE@0 'Note: Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.';
     ENDCOMP;

     FOOTNOTE1 ' ';
     FOOTNOTE2 ' ';
RUN;


 
/************************************************/
/***Sixth page of the report****/
/************************************************/
ods proclabel='Page 6';

PROC REPORT DATA=erss2025.schreptsummary2025_part2 NOWD HEADSKIP HEADLINE SPLIT='*'
     STYLE(COLUMN)=[font_size = 8pt  cellwidth = 0.75 in ]
     STYLE(HEADER LINES) = [font_size = 9pt]
	STYLE = [posttext  = "^n Table prepared by NALP, July 2026
      ^n NALP Summary Report data may vary slightly from the school-specific data published by the ABA Council because NALP's quality control process can result in changes which may not be reflected in ABA Council data. For more on this, see www.nalp.org/erssinfo."
		  font_size = 8pt];
	/*** STYLE = [posttext  = "^n Table prepared by NALP, August 2020 ***/
   /***          ^n Table reprinted by NALP on %sysfunc(left(%qsysfunc(today(),weekdate.))) " ***/
     /**        font_size = 8pt]; ***/
	
   
     TITLE1 "&NAME";
     TITLE2 "Class of 2025 Summary Report - Page 6";
   
     WHERE CODE = "&CODE" AND ANALVAR in ('SOURCE','TIME','ZSTATUS') ;

	 COLUMN AnalVar NewVar Count Percent;
          

     DEFINE Analvar / GROUP NOPRINT;
     DEFINE Newvar  / ORDER = INTERNAL GROUP '  ' FORMAT = $NEWVAR. STYLE = [CELLWIDTH = 1.75 IN JUST = LEFT];
     DEFINE Count   / SUM     'Number*Reported'   FORMAT = COMMA6.0 ;
     DEFINE Percent / SUM     '% of*Reported'     FORMAT = 6.1;
    

     COMPUTE BEFORE ANALVAR / STYLE = [font_weight = bold  font_size = 8pt];;
       LINE @2 ' ';
       LINE @2 Analvar $recode.;
     ENDCOMP;

     COMPUTE AFTER ANALVAR / STYLE = [font_weight = bold  font_size = 8pt  asis = on];
       LINE@10 Analvar     $subtotal.       
           +45 Count.sum   comma6.0
           +17 Percent.sum 6.1;
     ENDCOMP;

     COMPUTE AFTER;
  LINE@0 'Note: Figures are based on jobs for which the item was reported, and thus may not add to the total number of jobs. Timing of job offer figures exclude any graduates starting their own practice.';
     ENDCOMP;

     FOOTNOTE1 ' ';
     FOOTNOTE2 ' ';
RUN;

/************************************************/
/***seventh page of the report****/
/************************************************/

ods proclabel='Page 7';
PROC REPORT DATA=erss2025.schreptsummary2025_part2 NOWD HEADSKIP HEADLINE SPLIT='*'
     STYLE(COLUMN)=[font_size = 8pt  cellwidth = 0.75 in ]
     STYLE(HEADER LINES) = [font_size = 9pt]

	STYLE = [posttext  = "^n Table prepared by NALP, July 2026
      ^n NALP Summary Report data may vary slightly from the school-specific data published by the ABA Council because NALP's quality control process can result in changes which may not be reflected in ABA Council data. For more on this, see www.nalp.org/erssinfo."
		  font_size = 8pt];
	/*** STYLE = [posttext  = "^n Table prepared by NALP, August 2020 ***/
   /***          ^n Table reprinted by NALP on %sysfunc(left(%qsysfunc(today(),weekdate.))) " ***/
     /**        font_size = 8pt]; ***/
	 
 
     TITLE1 "&NAME";
     TITLE2 "Class of 2025 Summary Report - Page 7";
   
     WHERE CODE = "&CODE" AND ANALVAR in ( 'DURATION', 'LAW SCHOOL FUNDED') ;

	  COLUMN AnalVar NewVar ('Number of Jobs Reported as:' perm temp);
          

     DEFINE Analvar / GROUP NOPRINT;
     DEFINE Newvar  / ORDER = INTERNAL GROUP '  ' FORMAT = $NEWVAR. STYLE = [CELLWIDTH = 2.0 IN JUST = LEFT];
     DEFINE perm   / SUM     'Long-term (1+ years)'  width = 12 FORMAT = COMMA6.0 ;
	/**** DEFINE count   / SUM     'Number*of School Funded'   FORMAT = COMMA6.0 ; ***/
	/****DEFINE fixed   / SUM     'Number*of Fixed*Duration'   FORMAT = COMMA6.0 ; ***/
	 DEFINE temp   / SUM     'Short-term (Less than 1 year)' width = 12  FORMAT = COMMA6.0 ;
       

     COMPUTE BEFORE ANALVAR / STYLE = [font_weight = bold  font_size = 8pt];;
       LINE @2 ' ';
       LINE @2 Analvar $recode.;
     ENDCOMP;

     COMPUTE AFTER ANALVAR / STYLE = [font_weight = bold  font_size = 8pt  asis = on];
       LINE @10 Analvar     $subtotal.       
         
	        +42 perm.sum 
            +13 temp.sum;
     ENDCOMP;
	
     COMPUTE AFTER;
  LINE@0 'Note: Figures for job duration are based on jobs for which the item was reported, and thus may not add to the total number of jobs. The count of jobs funded by the law school is a total, regardless of duration.';
  ENDCOMP;

     FOOTNOTE1 ' ';
     FOOTNOTE2 ' ';

RUN;

%MEND SCHRPTS;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\10701_quinnipiac_summary2025.pdf'  accessible;
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

    %SCHRPTS (10701,   Quinnipiac University School of Law, '107' , CT);

 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\10702_uconn_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (10702,    University of Connecticut School of Law, '107' , CT);
 ods pdf close;
 
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\10703_yale_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (10703,    Yale Law School, '107' , CT);

 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\12001_umaine_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (12001, University of Maine School of Law, '120' , ME);

 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\12201_bostoncollege_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
	%SCHRPTS (12201, Boston College Law School, '122' , MA);
  
 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\12202_bostonu_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (12202, Boston University School of Law, '122' , MA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\12203_harvard_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (12203, Harvard Law School, '122' , MA);

   ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\12204_newengland_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (12204, New England Law | Boston,   '122' , MA);

  ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\12205_northeastern_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (12205, Northeastern University School of Law,  '122' , MA);

 ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\12206_suffolk_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (12206, Suffolk University Law School, '122' , MA);

 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\12207_westernnewengland_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (12207, Western New England University School of Law, '122' , MA);
 ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\12208_umass_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (12208, University of Massachusetts School of Law-Dartmouth, '122' , MA);

 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\13001_unewhampshire_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (13001, University of New Hampshire Franklin Pierce School of Law, '130' , NH);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\14001_rogerwilliams_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (14001, Roger Williams University School of Law, '140' , RI);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\14601_vermont_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (14601, Vermont Law School, '146' , VT);

ods pdf close;

 *  %SCHRPTS (23101, Rutgers School of Law--Camden, '231' , NJ);

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23102_rutgers_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (23102, Rutgers Law School , '231' , NJ);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23103_setonhall_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23103, Seton Hall Law School, '231' , NJ);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23301_albany_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (23301, Albany Law School, '233' , NY);
ods pdf close;
 
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23302_brooklyn_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23302, Brooklyn Law School, '233' , NY);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23303_columbia_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

    %SCHRPTS (23303, Columbia Law School, '233' ,NY);

ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23304_cornell_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23304, Cornell Law School, '233' , NY);

ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23305_fordham_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23305, Fordham University School of Law, '233' , NY);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23306_hofstra_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23306, Hofstra University Maurice A. Deane School of Law , '233' , NY);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23307_ubuffalo_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23307, University at Buffalo School of Law School, '249' , NY);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23308_newyorklaw_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23308, New York Law School , '233' , NY);
ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23309_nyu_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
 
    %SCHRPTS (23309, New York University School of Law , '233' , NY);

ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23310_pace_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23310, Elisabeth Haub School of Law at Pace University , '233' , NY);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23311_stjohns_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23311, St. Johns University School of Law , '233' , NY);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23312_syracuse_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23312, Syracuse University College of Law , '233' , NY);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23313_touro_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23313, Touro College - Jacob D. Fuchsberg Law Center , '233' , NY);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23314_cardozo_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23314, Benjamin N. Cardozo School of Law - Yeshiva University , '233' , NY);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23315_cuny_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23315, City University of New York School of Law , '233' , NY);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23901_pennstatedickinson_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
 
   %SCHRPTS (23901, Penn State Dickinson Law, '239' , PA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23902_duquesne_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23902, Duquesne University Thomas R. Kline School of Law, '239' , PA);
ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23903_upenn_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23903, University of Pennsylvania Carey Law School, '239' , PA);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23904_upitt_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23904, University of Pittsburgh School of Law, '239' , PA);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23905_temple_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23905, Temple University - James E. Beasley School of Law, '239' , PA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23906_villanova_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23906, Villanova University Charles Widger School of Law, '239' , PA);
ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23907_drexel_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23907, Drexel University Thomas R. Kline School of Law, '239' , PA);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23908_widenerpa_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (23908, Widener University Commonwealth Law School, '239' , PA);
ods pdf close;

**merged with dickinson, no longer reporting separately;
*ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\23909_pennstate_summary2025.pdf';
 *     ODS pdf STYLE= GrayscalePrinter pdftoc=1;
  * %SCHRPTS (23909, Penn State Law, '239' , PA);

   *ODS PDF CLOSE;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31401_uchicago_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31401, The University of Chicago Law School, '314' , IL);

ODS PDF CLOSE;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31402_depaul_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31402, DePaul University College of Law , '314' , IL);

ODS PDF CLOSE;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31403_uillinois_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (31403, University of Illinois College of Law , '314' , IL);


ODS PDF CLOSE;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31404_chicagokent_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31404, Chicago-Kent College of Law - Illinois Institute of Technology, '314' , IL);

ODS PDF CLOSE;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31405_uillinois-chicago_summary2024 .pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31405, University of Illinois Chicago School of Law, '314' , IL);

ODS PDF CLOSE;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31406_loyolachicago_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31406, Loyola University Chicago School of Law, '314' , IL);

ODS PDF CLOSE;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31407_northernillinois_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31407, Northern Illinois University College of Law, '314' , IL);
ODS PDF CLOSE;


  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31408_northwestern_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31408, Northwestern Pritzker School of Law, '314' , IL);
ODS PDF CLOSE;

 
  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31409_southernillinois_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31409, Southern Illinois University Simmons Law School , '314' , IL);

ODS PDF CLOSE;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31501_indianamaurer_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31501, Indiana University Maurer School of Law, '315' , IN);

ODS PDF CLOSE;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31502_indianamckinney_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31502, Indiana University Robert H. McKinney School of Law, '315' , IN);

ODS PDF CLOSE;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\31503_notredame_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (31503, Notre Dame Law School, '315' , IN);

ODS PDF CLOSE;

  * %SCHRPTS (31504, Valparaiso University School of Law, '315' , IN);
* ods pdf close;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\32301_udetroitmercy_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
%SCHRPTS (32301, University of Detroit Mercy School of Law, '323' , MI);

ODS PDF CLOSE;

 
  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\32302_michiganstate_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (32302, Michigan State University College of Law, '323' , MI);
ods pdf close;


  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\32303_umichigan_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (32303, The University of Michigan Law School, '323' , MI);
ods pdf close;


  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\32304_cooley_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (32304, Cooley Law School, '323' , MI);

ods pdf close;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\32305_waynestate_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (32305, Wayne State University Law School,  '323' , MI);
  ods pdf close;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\33601_uakron_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (33601, The University of Akron School of Law , '336' , OH);
  ods pdf close;


  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\33602_capital_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (33602, Capital University Law School , '336' , OH);

  ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\33603_casewestern_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (33603, Case Western Reserve University School of Law , '336' , OH);

  ods pdf close;



 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\33604_ucincinnati_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (33604, The University of Cincinnati Donald P. Klekamp College of Law , '336' , OH);

  ods pdf close;


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\33605_csu_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (33605, Cleveland State University College of Law , '336' , OH);
 ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\33606_udayton_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (33606, University of Dayton School of Law , '336' , OH);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\33607_ohionorthern_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (33607, Ohio Northern University Claude W. Pettit College of Law , '336' , OH);
ods pdf close;


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\33608_ohiostate_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (33608, The Ohio State University Moritz College of Law , '336' , OH);
ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\33609_utoledo_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (33609, University of Toledo College of Law , '336' , OH);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\35001_marquette_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (35001, Marquette University Law School, '350' , WI);

ods pdf close;

 
 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\35002_uwisconsin_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (35002, University of Wisconsin Law School, '350' , WI);
ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\41601_drake_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (41601, Drake University Law School, '416' , IA);
ods pdf close;


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\41602_uiowa_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
	%SCHRPTS (41602, University of Iowa College of Law,  '416' , IA);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\41701_ukansas_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (41701, University of Kansas School of Law , '417' , KS);
ods pdf close;


   
 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\41702_washburn_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (41702, Washburn University School of Law , '417' , KS);
ods pdf close;


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\42402_uminnesota_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (42402, University of Minnesota Law School , '424' , MN);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\42403_mitchellhamline_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (42403, Mitchell Hamline School of Law  , '424' , MN);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\42404_stthomasmn_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (42404, University of St. Thomas School of Law , '424' , MN);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\42601_umissouri-columbia_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (42601, University of Missouri School of Law, '426' , MO);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\42602_umissouri-kansascity_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (42602, University of Missouri-Kansas City School of Law , '426' , MO);

ods pdf close;


  * %SCHRPTS (42603, Saint Louis University School of Law, '426' , MO);


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\42604_washingtonu_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (42604, Washington University School of Law , '426' , MO);

ods pdf close;


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\42801_creighton_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (42801, Creighton University School of Law , '428' , NE);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\42802_unebraska_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (42802, University of Nebraska College of Law  , '428' , NE);


ods pdf close;


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\43501_unorthdakota_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;


   %SCHRPTS (43501, University of North Dakota School of Law , '435' , ND);
ods pdf close;



 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\44201_usouthdakota_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (44201, University of South Dakota School of Law , '442' , SD);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\50801_widenerde_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (50801, Widener University Delaware Law School,' 508' , DE);
ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\50901_american_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (50901, American University Washington College of Law , '509' , DC);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\50903_catholic_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (50903, The Catholic University of America Columbus School of Law, '509' , DC);
ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\50904_georgetown_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (50904, Georgetown University Law Center  , '509' , DC);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\50905_georgewashington_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (50905, The George Washington University Law School ,'509' , DC);
ods pdf close;
 
 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\50906_howard_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (50906, Howard University School of Law, '509' , DC);

ods pdf close;


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\50907_udc_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (50907, University of the District of Columbia David A. Clarke School of Law ,'509' , DC);


ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51001_uflorida_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (51001, University of Florida - Fredric G. Levin College of Law , '510' , FL);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51002_floridastate_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51002, The Florida State University College of Law, '510' , FL);


ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51003_umiami_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51003, University of Miami School of Law , '510' , FL);
ods pdf close;


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51004_novasoutheastern_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51004, Nova Southeastern University Shepard Broad College of Law, '510' , FL);
ods pdf close;


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51005_stetson_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (51005, Stetson University College of Law , '510' , FL);

ods pdf close;

 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51006_stthomasfl_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51006, St. Thomas University College of Law , '510' , FL);
ods pdf close;
 

      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51007, Florida Coastal School of Law     , '510' , FL);


 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51008_barry_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51008, Barry University Dwayne O. Andreas School of Law , '510' , FL);
 ods pdf close;
 
 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51009_floridam_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51009, Florida Agricultural And Mechanical University College of Law , '510' , FL);
 ods pdf close;

 
 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51010_floridainternational_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51010, Florida International University College of Law , '510' , FL);
 ods pdf close;


 
 ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51011_avemaria_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
    %SCHRPTS (51011, Ave Maria School of Law,  '510' , FL);

 ods pdf close;

  ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51012_jacksonville_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
    %SCHRPTS (51012, Jacksonville University College of Law,  '510' , FL);

 ods pdf close;



ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51101_emory_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
	 %SCHRPTS (51101, Emory University School of Law , '511' , GA);
ods pdf close;
   run;



ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51102_ugeorgia_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51102, University of Georgia School of Law , '511' , GA);

ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51103_georgiastate_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51103, Georgia State University College of Law , '511' , GA);


ods pdf close;
   run;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51104_mercer_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51104, Mercer University School of Law , '511' , GA);
ods pdf close;

run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\51105_johnmarshallatl_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (51105, John Marshall Law School - Atlanta, '511' , GA);

ods pdf close;
   run;

    %SCHRPTS (51106, Savannah Law School, '511' , GA);

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\52101_ubaltimore_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (52101, University of Baltimore School of Law  , '512' , MD);
ods pdf close;
   run;

  
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\52102_umaryland_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (52102, University of Maryland Francis King Carey School of Law  , '512'  , MD);

ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\53401_campbell_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (53401, Campbell University Norman A. Wiggins School of Law , '534' , NC);

  ods pdf close;
   run;
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\53402_duke_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (53402, Duke University School of Law ,       '534' , NC);

  ods pdf close;
   run;
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\53403_unorthcarolina_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (53403, University of North Carolina School of Law ,'534' , NC);

  ods pdf close;
   run;

*ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\53404_nccentral_summary2025.pdf';
 *     ODS pdf STYLE= GrayscalePrinter pdftoc=1;
  * %SCHRPTS (53404, North Carolina Central University School of Law , '534' , NC);

* ods pdf close;
  * run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\53405_wakeforest_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (53405, Wake Forest University School of Law, '534' , NC);
ods pdf close;
   run;

   *%SCHRPTS (53406, Charlotte School of Law , '534' , NC);

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\53407_elon_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (53407, Elon University School of Law ,'534' , NC);

ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54101_usouthcarolina_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54101, University of South Carolina School of Law, '541' , SC);

ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54103_charleston_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54103, Charleston School of Law      , '541' , SC);

ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54701_georgemason_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54701, George Mason University Antonin Scalia Law School , '547' , VA);
ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54702_urichmond_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54702, University of Richmond School of Law , '547' , VA);
ods pdf close;


*ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2022\54703_uvirginia_summary2022.pdf';
 *     ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   *%SCHRPTS (54703, University of Virginia School of Law  , '547' , VA);

*ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54704_washingtonlee_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54704, Washington and Lee University School of Law ,'547' , VA);


ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54705_williammary_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54705, William & Mary Law School      , '547' , VA);
ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54706_regent_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54706, Regent University School of Law      , '547' , VA);

ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54707_appalachian_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54707, Appalachian School of Law      , '547' , VA);
ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54708_liberty_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54708, Liberty University School of Law     , '547' , VA);
ods pdf close;
   run;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\54901_wvu_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (54901, West Virginia University College of Law  , '549' , WV);

ods pdf close;
   run;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\60101_ualabama_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (60101, The University of Alabama School of Law, '601' , AL);
ods pdf close;
   run;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\60102_samford_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (60102, Samford University Cumberland School of Law , '601' , AL);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\60103_faulkner_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (60103, Faulkner University Thomas Goode Jones School of Law , '601' , AL);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\61801_ukentucky_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (61801, University of Kentucky J. David Rosenberg College of Law  , '618' , KY);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\61802_ulouisville_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (61802, Louis D. Brandeis School of Law at the University of Louisville, '618' , KY);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\61803_northernky_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

    %SCHRPTS (61803, Northern Kentucky University Salmon P. Chase College of Law  , '618' , KY);
ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\62501_umississippi_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (62501, The University of Mississippi School of Law ,'625' , MS);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\62502_misscollege_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (62502, Mississippi College School of Law  , '625' , MS);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\64301_umemphis_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (64301, The University of Memphis Cecil C. Humphreys School of Law, '643' , TN);
 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\64302_utennessee_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
%SCHRPTS (64302, University of Tennessee College of Law    , '643' , TN);
ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\64303_vanderbilt_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (64303, Vanderbilt University Law School      , '643' , TN);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\64304_belmont_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (64304, Belmont University College of Law, '643' , TN);
   
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\64305_lincolnmemorial_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (64305, Lincoln Memorial University Duncan School of Law, '643' , TN);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\70401_uarkansas_fayetteville_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (70401, University of Arkansas School of Law, '704' , AR);

ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\70402_uarkansas_littlerock_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (70402, University of Arkansas at Little Rock William H. Bowen School of Law, '704' , AR);

ods pdf close;

 
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\71901_louisianastate_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
    %SCHRPTS (71901, Louisiana State University Paul M. Hebert Law Center , '719' , LA);
ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\71902_loyolaneworleans_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (71902, Loyola University New Orleans College of Law , '719' , LA);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\71903_southernu_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (71903, Southern University Law Center  , '719' , LA);

   ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\71904_tulane_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (71904, Tulane Law School   , '719' , LA);
   ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\73701_uoklahoma_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (73701, University of Oklahoma College of Law , '737' , OK);
   ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\73702_oklahomacity_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (73702, Oklahoma City University School of Law , '737' , OK);
  ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\73704_utulsa_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (73704, University of Tulsa College of Law  , '737' , OK);

  ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74401_baylor_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (74401, Baylor Law School , '744' , TX);

  ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74402_uhouston_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (74402, University of Houston Law Center , '744' , TX);

  ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74403_stmarys_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (74403, St. Marys University School of Law , '744' , TX);

  ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74404_smu_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (74404, SMU Dedman School of Law , '744' , TX);

  ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74405_southtexas_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (74405, South Texas College of Law Houston, '744' , TX);

  ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74406_utexas_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (74406, The University of Texas School of Law , '744' , TX);

  ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74407_texassouthern_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (74407, Texas Southern University - Thurgood Marshall School of Law , '744' , TX);
  ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74408_texastech_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (74408, Texas Tech University School of Law, '744' , TX);
  ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74409_texasam_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (74409, Texas A&M University School of Law , '744' , TX);

  ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\74410_untdallas_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (74410, UNT Dallas College of Law, '744' ,TX);
  ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\80301_uarizona_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (80301, University of Arizona James E. Rogers College of Law , '803' , AZ);
 ods pdf close;

  
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\80302_arizonastate_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (80302, Sandra Day O Connor College of Law at Arizona State University, '803' , AZ);
 ods pdf close;

  * %SCHRPTS (80305, Arizona Summit Law School, '803' , AZ);

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\80601_ucolorado_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (80601, University of Colorado Law School, '806' , CO);
 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\80602_udenver_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (80602, University of Denver Sturm College of Law  , '806' , CO);
 ods pdf close;
 
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\81301_uidaho_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (81301, University of Idaho College of Law , '813' , ID);
ods pdf close;


  * %SCHRPTS (81302, Concordia University School of Law , '813' , ID);

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\82701_umontana_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

     %SCHRPTS (82701,  Alexander Blewett III School of Law at the University of Montana, '827' , MT);

 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\82901_unevada_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (82901, University of Nevada Las Vegas - William S. Boyd School of Law, '829' , NV);

 ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\83201_unewmexico_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (83201, University of New Mexico School of Law, ' 832' , NM);

 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\84501_byu_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (84501, Brigham Young University J. Reuben Clark Law School, '845' , UT);
 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\84502_uutah_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (84502, University of Utah S.J. Quinney College of Law, '845' , UT);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\85101_uwyoming_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (85101, University of Wyoming College of Law , '851' , WY);

 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90501_ucberkeley_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

 
   %SCHRPTS (90501, University of California - Berkeley School of Law , '905', CA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90502_ucdavis_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (90502, University of California - Davis School of Law (King Hall),  '905', CA);
 ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90503_ucla_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90503, UCLA School of Law ,  '905', CA);
ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90504_ucsf_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90504, University of California College of the Law San Francisco,  '905', CA);
ods pdf close;

 
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90505_cawestern_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90505, California Western School of Law ,  '905', CA);

ods pdf close;

*ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2022\90506_goldengate_summary2022.pdf';
  *    ODS pdf STYLE= GrayscalePrinter pdftoc=1;
 *  %SCHRPTS (90506, Golden Gate University School of Law ,  '905', CA);

*ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90507_loyolala_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90507, Loyola Law School ,    '905', CA);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90508_upacific_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90508, University of the Pacific - McGeorge School of Law ,  '905', CA);

ods pdf close;
 
ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90509_pepperdine_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90509, Pepperdine University Caruso School of Law ,   '905', CA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90510_usandiego_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90510, University of San Diego School of Law ,  '905', CA);

ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90511_usanfrancisco_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90511, University of San Francisco School of Law, '905'  CA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90512_usantaclara_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90512, Santa Clara University School of Law ,   '905', CA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90513_usoutherncalifornia_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90513, University of Southern California Gould School of Law,   '905', CA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90514_southwestern_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (90514, Southwestern Law School ,  '905', CA);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90515_stanford_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (90515, Stanford Law School ,  '905', CA);
ods pdf close;

*   %SCHRPTS (90516, Whittier Law School,   '905', CA);


 *  %SCHRPTS (90517, Thomas Jefferson School of Law ,  '905', CA);

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90518_chapman_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90518, The Dale E. Fowler School of Law at Chapman University, '905', CA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90519_westernstate_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (90519, Western State University College of Law , '905', CA);
ods pdf close;

   %SCHRPTS (90520, University of LaVerne College of Law, '905', CA);

*ods pdf file = 'H:\WORK\DANIELLE\ERSS School Reports\2019\90521_ucirvine_summary.pdf';
/**this indicates the style of the report, can be changed, Pearl is the default in 9.4 if not specified***/
*ODS pdf STYLE= GrayscalePrinter;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\90521_ucirvine_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;

   %SCHRPTS (90521, University of California - Irvine School of Law, '905', CA);
ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\91201_uhawaii_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (91201, University of Hawaii at Manoa William S. Richardson School of Law, '912', HI);
ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\93801_lewisclark_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (93801, Lewis & Clark Law School , '938', OR);
ods pdf close;



ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\93802_uoregon_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (93802, University of Oregon School of Law  , '938', OR);

ods pdf close;


ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\93803_willamette_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (93803, Willamette University College of Law  , '938', OR);

ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\94801_gonzaga_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (94801, Gonzaga University School of Law , '948', WA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\94802_seattleu_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (94802, Seattle University School of Law  , '948', WA);
ods pdf close;

ods pdf file = 'C:\Users\OksanaPoulis\NALP\Research - Documents\ERSS School Reports\2025\94803_uwashington_summary2025.pdf';
      ODS pdf STYLE= GrayscalePrinter pdftoc=1;
   %SCHRPTS (94803, University of Washington School of Law  , '948', WA);

   ods pdf close;
*/
 
     ods pdf file = 'C:\myDocuments\ed50901_2017.pdf';
	     %SCHRPTS (50901, American University - Washington College of Law , '509', DC);
	
	            	 ods pdf close;

	 *  ods pdf file = 'C:\myDocuments\ed33604_2017.pdf';
	  *       %SCHRPTS (33604, The University of Cincinnati College of Law , '336', OH);
	*	  ods pdf close;


		*  ods pdf file = 'C:\myDocuments\ed51009_2017.pdf';
        *    %SCHRPTS (51009, Florida A and M University College of Law , '510', FL);
		*  ods pdf close;

		*   ods pdf file = 'C:\myDocuments\ed23303_2017.pdf';
		   *  %SCHRPTS (23303, Columbia University School of Law, '233',NY);
         
		 *  ods pdf close;

           *   ods pdf file = 'C:\myDocuments\ed90503_2017.pdf';
			*    %SCHRPTS (90503, UCLA School of Law ,  '905', CA);
			     				   			  		
		    *  ods pdf close;
	
 * ods csvall file = 'C:\mydocuments\ed31402_det.csv';
  *  %SCHRPTS (31402, DePaul University College of Law , '314', IL);
   * ods csvall close;

