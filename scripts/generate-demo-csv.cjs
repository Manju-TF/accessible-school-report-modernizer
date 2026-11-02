const fs = require("fs");
const path = require("path");

const headers = [
  "code", "sex3", "minstat", "jobcat1", "jobftpt", "empgen", "firm1",
  "lfjob", "jobreg", "locationflag", "jobst", "source", "time1", "status",
  "duration", "schoolfund", "salftperm", "emptype1", "classyear"
];

const schools = ["23306", "23309", "23903", "23904", "31401", "31408", "51001", "51012"];
const years = [2024, 2025, 2026];
const sex = ["M", "F", "N", "W", "X", "ND"];
const minority = ["MINOR", "NONMIN"];
const jobcats = ["LJD", "NLJD", "NLP", "NLO", "WUNK", "ADVD", "UDEF", "USKW", "UNWK", "UNKN"];
const workTypes = ["FULL", "PART", ""];
const employers = ["ACAD", "BUS", "FIRM", "GOVT", "CLERK", "PUBINT", "EMPUNK", ""];
const firmSizes = ["S", "1", "2", "3", "4", "5", "6", "7", "8", "OTH"];
const lawFirmJobs = ["PARTNER", "ASSOC", "ADMIN", "OTHNL", "STATTY", ""];
const regions = ["0", "1", "2", "3", "4", "5", "X"];
const locations = ["INSTATE", "OUTSTATE", "LOCAL", "NATIONAL", ""];
const jobStatuses = ["EMP", "UNEMP", "CONTINUE", "STARTUP", ""];
const sources = ["OCI", "JOBFAIR", "NETWORK", "OTHER", "SCHOOL", ""];
const times = ["BEFGRAD", "ATGRAD", "AFTGRAD", "AFTGRD", ""];
const statuses = ["FOUND", "SOUGHT", "ACCEPTED", "PENDING", ""];
const durations = ["LT1", "GT1LT6", "GT6LT12", "GT12", ""];
const schoolFunds = ["YES", "NO", ""];
const employmentTypes = ["JCLOGV", "JCINGV", "JCOTGV", "JCUNGV", "JC", "OTH", ""];

function csvCell(value) {
  const text = String(value ?? "");
  return /[",\r\n]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
}

const lines = [headers.join(",")];
for (let i = 0; i < 1000; i++) {
  const year = years[Math.floor(i / 334) % years.length];
  const jobcat = jobcats[i % jobcats.length];
  const jobftpt = workTypes[i % workTypes.length];
  const salaryEligible = ["LJD", "NLJD", "NLP", "NLO"].includes(jobcat) && jobftpt === "FULL";
  const salary = salaryEligible && i % 4 !== 0 ? 60000 + (i * 137 % 80000) : (i % 9 === 0 ? 50000 + (i * 91 % 40000) : "");
  const row = [
    schools[i % schools.length], sex[i % sex.length], minority[i % minority.length], jobcat,
    jobftpt, employers[i % employers.length], firmSizes[i % firmSizes.length],
    lawFirmJobs[i % lawFirmJobs.length], regions[i % regions.length], locations[i % locations.length],
    jobStatuses[i % jobStatuses.length], sources[i % sources.length], times[i % times.length],
    statuses[i % statuses.length], durations[i % durations.length], schoolFunds[i % schoolFunds.length],
    salary, employmentTypes[i % employmentTypes.length], year
  ];
  lines.push(row.map(csvCell).join(","));
}

const output = path.join(__dirname, "..", "data", "sample-workbooks", "demo-1000-graduates-multiple-years.csv");
fs.mkdirSync(path.dirname(output), { recursive: true });
fs.writeFileSync(output, lines.join("\r\n") + "\r\n", "utf8");
console.log(output);
console.log(`Rows: ${lines.length - 1}`);
