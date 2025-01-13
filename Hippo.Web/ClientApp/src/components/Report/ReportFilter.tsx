import React from "react";
import HipButton from "../../Shared/HipComponents/HipButton";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faRunning } from "@fortawesome/free-solid-svg-icons";

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
      <div className="row col-12 col-md-3">
        <div className="form-group">
          <label htmlFor="fieldStartDate">Start Date:</label>
          <input
            className="form-control"
            id="fieldStartDate"
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
          />
        </div>
        <div className="form-group">
          <label htmlFor="fieldEndDate">End Date:</label>
          <input
            className="form-control"
            id="fieldEndDate"
            type="date"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
          />
        </div>
        <div className="form-group">
          <label htmlFor="fieldFilterType">Filter Type:</label>
          <select
            className="form-control form-select"
            id="fieldFilterType"
            value={filterType}
            onChange={(e) => setFilterType(e.target.value)}
          >
            <option value="PaymentDate">Payment Date</option>
            <option value="OrderExpiryDate">Order Expiry Date</option>
            <option value="OrderInstallmentDate">Order Installment Date</option>
            <option value="OrderCreationDate">Order Creation Date</option>
          </select>
        </div>
      </div>
      <br />
      <HipButton onClick={handelRun} className="btn btn-primary">
        Run Report <FontAwesomeIcon icon={faRunning} />
      </HipButton>
    </div>
  );
};

export default ReportFilter;
