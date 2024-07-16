import React from "react";
import { authenticatedFetch } from "../../util/api";
import { ChartStringValidationModel } from "../../types";
import { faCircleNotch } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

interface ChartStringValidationProps {
  chartString: string;
}

type ChartStringStatus = "loading" | "error" | "success" | undefined;

const ChartStringValidation: React.FC<ChartStringValidationProps> = ({
  chartString,
}) => {
  const [chartStringValidation, setChartStringValidation] =
    React.useState<ChartStringValidationModel>(null);
  const [status, setStatus] = React.useState<ChartStringStatus>(undefined);

  React.useEffect(() => {
    if (!chartString) {
      setStatus(undefined);
      return;
    }

    const validateChartString = async () => {
      setStatus("loading");
      try {
        let response = await authenticatedFetch(
          `/api/order/validateChartString/${chartString}/Debit`,
          {
            method: "GET",
          },
        );
        if (response.ok) {
          const result: ChartStringValidationModel = await response.json();
          setChartStringValidation(result);
          setStatus("success");
        } else {
          setStatus("error");
        }
      } catch (error) {
        setStatus("error");
      }
    };

    validateChartString();
  }, [chartString]);

  // default to showing loading state on mount
  if (status === "loading" || status === undefined) {
    return (
      <span>
        <FontAwesomeIcon icon={faCircleNotch} spin={true} />
      </span>
    );
  }

  if (status === "error") {
    return <span>Error loading chart string validation</span>;
  }

  return (
    <span>
      {chartStringValidation.isValid ? (
        <span style={{ color: "green" }}>
          <i className="fas fa-check"></i> {chartStringValidation.description}
        </span>
      ) : (
        <span style={{ color: "red" }}>
          <i className="fas fa-times"></i> {chartStringValidation.message}
        </span>
      )}
      {chartStringValidation.warning && (
        <>
          <br />
          <span style={{ color: "orange" }}>
            <i className="fas fa-exclamation-triangle"></i>{" "}
            {chartStringValidation.warning}
          </span>
        </>
      )}
    </span>
  );
};

export default ChartStringValidation;
