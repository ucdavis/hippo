import React, { useState, useEffect } from "react";
import { ChartStringValidationModel, FinancialDetailModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { useParams } from "react-router-dom";
import { usePromiseNotification } from "../../util/Notifications";

declare const window: Window &
  typeof globalThis & {
    Finjector: any;
  };

const FinancialDetail: React.FC = () => {
  const [financialDetail, setFinancialDetail] =
    useState<FinancialDetailModel | null>(null);
  const { cluster: clusterName } = useParams();
  const [notification, setNotification] = usePromiseNotification();
  const [chartStringValidation, setChartStringValidation] =
    useState<ChartStringValidationModel | null>(null);

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
    alert(name + " " + value);
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

  const lookupcoa = async () => {
    const chart = await window.Finjector.findChartSegmentString();

    if (chart.status === "success") {
      //alert("Chart Segment: " + chart.data);

      financialDetail.chartString = chart.data;

      setFinancialDetail((prevFinancialDetail) => ({
        ...prevFinancialDetail,
        chartString: chart.data,
      }));

      //call api/order/validate-chart-string/{chart.data} to validate the chart string
      let response = await authenticatedFetch(
        `/api/order/validate-chart-string/${chart.data}`,
        {
          method: "GET",
        },
      );
      //console.log(response);
      if (response.ok) {
        const result = await response.json();
        console.log(result);
        setChartStringValidation(result);
      }

      //Copied from Harvest. Keeping for now in case we call an api end point to validate the chart string here instead of in the backend
      // const response = await authenticatedFetch(
      //   `/api/financialaccount/get?account=${chart.data}`
      // );

      // if (response.ok) {
      //   if (response.status === 204) {
      //     alert("Account Selected is not valid");
      //     return;
      //   } else {
      //     var result = await response.json();

      //     if (result.success === false) {
      //       alert("Account Selected is not valid: " + result.error);
      //       return;
      //     }

      //     const accountInfo: ProjectAccount = result;

      //     if (accounts.some((a) => a.number === accountInfo.number)) {
      //       alert("Account already selected -- choose a different account");
      //       return;
      //     } else {
      //       alert(undefined);
      //     }

      //     if (accounts.length === 0) {
      //       // if it's our first account, default to 100%
      //       accountInfo.percentage = 100.0;
      //     }

      //     setAccounts([...accounts, accountInfo]);
      //   }
      //}
    } else {
      alert("Failed!");
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
        <div className="form-group">
          <label htmlFor="name">Update Financial API Key:</label>
          <input
            type="text"
            className="form-control"
            id="financialSystemApiKey"
            name="financialSystemApiKey"
            value={financialDetail.financialSystemApiKey}
            onChange={handleInputChange}
          />
        </div>

        <div className="form-group">
          <label htmlFor="chartString">Chart String:</label>{" "}
          {financialDetail.chartString && (
            <a
              href={`https://finjector.ucdavis.edu/Details/${financialDetail.chartString}`}
              target="_blank"
              rel="noreferrer"
            >
              {financialDetail.chartString}
            </a>
          )}
          <input
            type="text"
            className="form-control"
            id="chartString"
            name="chartString"
            value={financialDetail.chartString}
            onChange={handleInputChange}
            required
          />
          <button className="btn btn-primary" onClick={lookupcoa} type="button">
            Pick Chart String
          </button>
          {chartStringValidation && (
            <div>
              <div>Chart String Validation:</div>
              <div>
                Is Valid: {chartStringValidation.isValid ? "Yes" : "No"}
              </div>
              <div>Description: {chartStringValidation.description}</div>
              {chartStringValidation.accountManager && (
                <div>
                  <div>
                    Account Manager: {chartStringValidation.accountManager}
                  </div>
                  <div>
                    Account Manager Email:{" "}
                    {chartStringValidation.accountManagerEmail}
                  </div>
                </div>
              )}
              {chartStringValidation.message && (
                <div>Message: {chartStringValidation.message}</div>
              )}

              {chartStringValidation.warning && (
                <div>Warning: {chartStringValidation.warning}</div>
              )}
            </div>
          )}
        </div>

        <div className="form-group">
          <label htmlFor="financialSystemApiSource">API Source:</label>
          <input
            type="text"
            className="form-control"
            id="financialSystemApiSource"
            name="financialSystemApiSource"
            value={financialDetail.financialSystemApiSource}
            onChange={handleInputChange}
            required
          />
        </div>

        <div className="form-group">
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
        <button
          className="btn btn-primary"
          disabled={notification.pending}
          type="submit"
        >
          Submit
        </button>
      </form>
    </div>
  );
};

export default FinancialDetail;
