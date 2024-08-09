import React from "react";
import { authenticatedFetch } from "../../util/api";
import { ChartStringValidationModel } from "../../types";
import {
  faCheck,
  faCircleNotch,
  faExclamationTriangle,
  faTimes,
} from "@fortawesome/free-solid-svg-icons";
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
  const [status, setStatus] = React.useState<ChartStringStatus>();

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
    <div>
      {chartStringValidation.isValid ? (
        <span className="text-success">
          <FontAwesomeIcon icon={faCheck} /> {chartStringValidation.description}
        </span>
      ) : (
        <span className="text-danger">
          <FontAwesomeIcon icon={faTimes} /> {chartStringValidation.message}
        </span>
      )}
      {chartStringValidation.warning && (
        <>
          <br />
          <span className="text-warning">
            <FontAwesomeIcon icon={faExclamationTriangle} />{" "}
            {chartStringValidation.warning}
          </span>
        </>
      )}
    </div>
  );
};

export default ChartStringValidation;
