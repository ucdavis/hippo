import React from "react";

interface ReportFilterProps {
  runReport: () => void;
  startDate?: string;
  setStartDate: React.Dispatch<React.SetStateAction<string>>;
  endDate?: string;
  setEndDate: React.Dispatch<React.SetStateAction<string>>;
  filterType: string;
  setFilterType: React.Dispatch<React.SetStateAction<string>>;
}

const ReportFilter: React.FC<ReportFilterProps> = ({
  runReport,
  startDate,
  setStartDate,
  endDate,
  setEndDate,
  filterType,
  setFilterType,
}) => {
  const handelRun = async () => {
    await runReport();
  };

  return (
    <div>
      <div>
        <label>Start Date:</label>
        <input
          type="date"
          value={startDate}
          onChange={(e) => setStartDate(e.target.value)}
        />
      </div>
      <div>
        <label>End Date:</label>
        <input
          type="date"
          value={endDate}
          onChange={(e) => setEndDate(e.target.value)}
        />
      </div>
      <div>
        <label>Filter Type:</label>
        <select
          value={filterType}
          onChange={(e) => setFilterType(e.target.value)}
        >
          <option value="PaymentDate">Payment Date</option>
          <option value="OrderExpiryDate">Order Expiry Date</option>
          <option value="OrderInstallmentDate">Order Installment Date</option>
          <option value="OrderCreationDate">Order Creation Date</option>
        </select>
      </div>
      <button onClick={handelRun}>Run Report</button>
    </div>
  );
};

export default ReportFilter;
