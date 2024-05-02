import React, { useState, useEffect } from "react";
import { FinancialDetailModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { useParams } from "react-router-dom";
import { usePromiseNotification } from "../../util/Notifications";

const FinancialDetail: React.FC = () => {
  const [financialDetail, setFinancialDetail] =
    useState<FinancialDetailModel | null>(null);
  const { cluster: clusterName } = useParams();
  const [notification, setNotification] = usePromiseNotification();

  useEffect(() => {
    // Fetch financial detail data from API or any other data source
    // and set it to the state
    const fetchFinancialDetail = async () => {
      try {
        const response = await fetch(
          `/api/${clusterName}/admin/FinancialDetails`,
        ); // Replace with your API endpoint
        const data = await response.json();
        setFinancialDetail(data);
      } catch (error) {
        console.error("Error fetching financial detail:", error);
      }
    };

    fetchFinancialDetail();
  }, [clusterName]);

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = event.target;
    setFinancialDetail((prevFinancialDetail) => ({
      ...prevFinancialDetail,
      [name]: value,
    }));
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    const request = authenticatedFetch(
      `/api/${clusterName}/admin/UpdateFinancialDetails`,
      {
        method: "POST",
        body: JSON.stringify(financialDetail),
      },
    );
    setNotification(
      request,
      "Updating Financial Details",
      "Financial Details Updated",
      async (r) => {
        if (r.status === 400) {
          const errors = await parseBadRequest(await request);
          return errors;
        } else {
          return "An error happened, please try again.";
        }
      },
    );
    if ((await request).ok) {
      // refresh the page
      window.location.reload();
    }
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
          <label htmlFor="name">Update Financial API Key:</label>
          <input
            type="text"
            id="financialSystemApiKey"
            name="financialSystemApiKey"
            value={financialDetail.financialSystemApiKey}
            onChange={handleInputChange}
          />
        </div>

        <div>
          <label htmlFor="chartString">Chart String:</label>
          <input
            type="text"
            id="chartString"
            name="chartString"
            value={financialDetail.chartString}
            onChange={handleInputChange}
            required
          />
        </div>

        <div>
          <label htmlFor="financialSystemApiSource">API Source:</label>
          <input
            type="text"
            id="financialSystemApiSource"
            name="financialSystemApiSource"
            value={financialDetail.financialSystemApiSource}
            onChange={handleInputChange}
            required
          />
        </div>

        <div>
          <label htmlFor="autoApprove">Auto Approve:</label>
          <input
            type="checkbox"
            id="autoApprove"
            name="autoApprove"
            checked={financialDetail.autoApprove}
            onChange={(e) =>
              setFinancialDetail((prevFinancialDetail) => ({
                ...prevFinancialDetail,
                autoApprove: e.target.checked,
              }))
            }
          />
        </div>
        <button disabled={notification.pending} type="submit">
          Save
        </button>
      </form>
    </div>
  );
};

export default FinancialDetail;
