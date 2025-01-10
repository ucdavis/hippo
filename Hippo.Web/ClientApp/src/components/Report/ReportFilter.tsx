import React, { useState } from "react";

interface ReportFilterProps {
  onFilterChange: (startDate: string, endDate: string, option: string) => void;
  defaultStartDate?: string;
}

const ReportFilter: React.FC<ReportFilterProps> = ({
  onFilterChange,
  defaultStartDate,
}) => {
  const [startDate, setStartDate] = useState(defaultStartDate || "");
  const [endDate, setEndDate] = useState("");
  const [option, setOption] = useState("");

  const handleFilterChange = () => {
    onFilterChange(startDate, endDate, option);
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
        <label>Options:</label>
        <select value={option} onChange={(e) => setOption(e.target.value)}>
          <option value="">Select an option</option>
          <option value="PaymentDate">Payment Date</option>
          <option value="option2">Option 2</option>
          <option value="option3">Option 3</option>
        </select>
      </div>
      <button onClick={handleFilterChange}>Run Report</button>
    </div>
  );
};

export default ReportFilter;
