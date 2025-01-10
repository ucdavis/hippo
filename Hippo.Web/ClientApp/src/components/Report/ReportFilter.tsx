import React, { useState } from "react";

interface ReportFilterProps {
  onFilterChange: (startDate: string, endDate: string, option: string) => void;
  startDate?: string;
  setStartDate: React.Dispatch<React.SetStateAction<string>>;
  endDate?: string;
  setEndDate: React.Dispatch<React.SetStateAction<string>>;
  filterType: string;
  setFilterType: React.Dispatch<React.SetStateAction<string>>;
}

const ReportFilter: React.FC<ReportFilterProps> = ({
  onFilterChange,
  startDate,
  setStartDate,
  endDate,
  setEndDate,
  filterType,
  setFilterType,
}) => {
  const [option, setOption] = useState("");

  const handleFilterChange = async () => {
    await onFilterChange(startDate, endDate, option);
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
        <label>FIlter Type:</label>
        <select
          value={filterType}
          onChange={(e) => setFilterType(e.target.value)}
        >
          <option value="PaymentDate">Payment Date</option>
          <option value="OrderExpiryDate">Order Expiry Date</option>
          <option value="OrderInstallmentDate">Order Installment Date</option>
        </select>
      </div>
      <button onClick={handleFilterChange}>Run Report</button>
    </div>
  );
};

export default ReportFilter;
