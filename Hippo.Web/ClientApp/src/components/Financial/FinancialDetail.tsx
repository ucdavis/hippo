import React, { useState, useEffect } from "react";
import { ChartStringValidationModel, FinancialDetailModel } from "../../types";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { useParams } from "react-router-dom";
import { usePromiseNotification } from "../../util/Notifications";
import { Card, CardBody, CardTitle } from "reactstrap";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipButton from "../../Shared/HipComponents/HipButton";
import HipBody from "../../Shared/Layout/HipBody";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";

declare const window: Window &
  typeof globalThis & {
    Finjector: any;
  };

export const FinancialDetail = () => {
  const [financialDetail, setFinancialDetail] =
    useState<FinancialDetailModel>();
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
          `/api/${clusterName}/financial/FinancialDetails`,
        ); // Replace with your API endpoint
        const data = await response.json();
        setFinancialDetail(data);
        await validateChartString(data.chartString);
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
      `/api/${clusterName}/financial/UpdateFinancialDetails`,
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
      // refresh the data
      const data = await (await request).json();
      setFinancialDetail(data);
      await validateChartString(data.chartString);
    }
  };

  const lookupChartString = async () => {
    const chart = await window.Finjector.findChartSegmentString();

    if (chart.status === "success") {
      financialDetail.chartString = chart.data;

      setFinancialDetail((prevFinancialDetail) => ({
        ...prevFinancialDetail,
        chartString: chart.data,
      }));

      await validateChartString(chart.data);
    } else {
      alert("Failed!");
    }
  };

  const validateChartString = async (chartString: string) => {
    let response = await authenticatedFetch(
      `/api/order/validateChartString/${chartString}/Credit`,
      {
        method: "GET",
      },
    );

    if (response.ok) {
      const result = await response.json();
      setChartStringValidation(result);
      if (result.chartString) {
        setFinancialDetail((prevFinancialDetail) => ({
          ...prevFinancialDetail,
          chartString: result.chartString,
        }));
      }
    }
  };

  const Title = <HipTitle title="Details" subtitle="Financial" />;
  if (!financialDetail) {
    return (
      <HipMainWrapper>
        {Title}
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  }

  return (
    <HipMainWrapper>
      {Title}
      <HipBody>
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
            <label htmlFor="financialSystemApiSource">API Source:</label>
            <input
              type="text"
              className="form-control"
              id="financialSystemApiSource"
              name="financialSystemApiSource"
              value={financialDetail.financialSystemApiSource}
              onChange={handleInputChange}
              required
              maxLength={50}
            />
          </div>

          <div className="form-group">
            <label htmlFor="autoApprove">Auto Approve:</label>{" "}
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
            <div className="input-group">
              <input
                type="text"
                className="form-control"
                id="chartString"
                name="chartString"
                value={financialDetail.chartString}
                onChange={handleInputChange}
                onBlur={(e) => {
                  validateChartString(e.target.value);
                }}
                required
                maxLength={128}
              />
              <HipButton onClick={lookupChartString}>
                <i className="fas fa-search"></i>
              </HipButton>
            </div>
            {!financialDetail.isSlothValid && (
              <div>
                <br />
                <Card className="card-danger">
                  <CardTitle>
                    <h2>The Sloth settings are not valid!</h2>
                  </CardTitle>
                  <CardBody>
                    <div>
                      The Sloth settings are not valid. This could be the API
                      Source Name is wrong, or it doesn't match the Financial
                      API Key's team's settings.
                    </div>
                  </CardBody>
                </Card>
              </div>
            )}
            {chartStringValidation && (
              <div>
                <br />
                <Card
                  className={
                    chartStringValidation.isValid
                      ? "card-center"
                      : "card-danger"
                  }
                >
                  <CardTitle>
                    <h3>Chart String Details:</h3>
                  </CardTitle>
                  <CardBody>
                    <div>
                      Is Valid: {chartStringValidation.isValid ? "Yes" : "No"}
                    </div>
                    <div>Description: {chartStringValidation.description}</div>
                    {chartStringValidation.accountManager && (
                      <div>
                        <div>
                          Account Manager:{" "}
                          {chartStringValidation.accountManager}
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
                  </CardBody>
                </Card>
              </div>
            )}
          </div>
          <br />
          <HipButton disabled={notification.pending} type="submit">
            Submit
          </HipButton>
        </form>
      </HipBody>
    </HipMainWrapper>
  );
};
