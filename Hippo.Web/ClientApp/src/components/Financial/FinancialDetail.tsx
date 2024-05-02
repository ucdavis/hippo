import React, { useState, useEffect } from "react";
import { FinancialDetailModel } from "../../types";

const FinancialDetail: React.FC = () => {
  const [financialDetail, setFinancialDetail] =
    useState<FinancialDetailModel | null>(null);

  useEffect(() => {
    // Fetch financial detail data from API or any other data source
    // and set it to the state
    const fetchFinancialDetail = async () => {
      try {
        const response = await fetch("/api/caesfarm/admin/FinancialDetails"); // Replace with your API endpoint
        const data = await response.json();
        setFinancialDetail(data);
      } catch (error) {
        console.error("Error fetching financial detail:", error);
      }
    };

    fetchFinancialDetail();
  }, []);

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target;
    setFinancialDetail((prevFinancialDetail) => ({
      ...prevFinancialDetail,
      [name]: value,
    }));
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    // Send updated financial detail data to API or any other data source
    // for saving changes
    console.log("Updated financial detail:", financialDetail);
  };

  if (!financialDetail) {
    return <div>Loading...</div>;
  }

  return (
    <div>
      <h1>Financial Detail</h1>
      <form onSubmit={handleSubmit}>
        <div>
          <div>Financial API Key: {financialDetail.maskedApiKey}</div>
        </div>
        <div>
          <label htmlFor="name">Fund:</label>
          <input
            type="text"
            id="name"
            name="name"
            value={financialDetail.financialSystemApiKey}
            onChange={handleInputChange}
          />
        </div>

        <div>
          <label htmlFor="chartstring">Chart String:</label>
          <input
            type="text"
            id="chartstring"
            name="chartstring"
            value={financialDetail.chartString}
            onChange={handleInputChange}
          />
        </div>

        {/* Add more input fields for other properties */}
        <button type="submit">Save</button>
      </form>
    </div>
  );
};

export default FinancialDetail;
