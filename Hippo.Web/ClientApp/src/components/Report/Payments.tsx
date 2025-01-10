import { useEffect, useState } from "react";
import { PaymentReportModel } from "../../types";
import { useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";

import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";

import HipLoadingTable from "../../Shared/LoadingAndErrors/HipLoadingTable";
import { PaymentsTable } from "./Tables/PaymentsTable";

export const Payments = () => {
  const [payments, setPayments] = useState<PaymentReportModel[]>();
  const { cluster } = useParams();
  const [runningReport, setRunningReport] = useState(false);

  useEffect(() => {
    setPayments(undefined);
  }, []);

  // useEffect(() => {
  //   //preload?
  //   const fetchPayments = async () => {
  //     const response = await authenticatedFetch(
  //       `/api/${cluster}/report/payments`,
  //     );

  //     if (response.ok) {
  //       const data = await response.json();
  //       setPayments(data);
  //     } else {
  //       alert("Error fetching payments");
  //     }
  //   };

  //   fetchPayments();
  // }, [cluster]);

  const fetchPayments = async () => {
    setRunningReport(true);
    const response = await authenticatedFetch(
      `/api/${cluster}/report/payments`,
    );

    if (response.ok) {
      const data = await response.json();
      setPayments(data);
    } else {
      alert("Error fetching payments");
    }
    setRunningReport(false);
  };

  const Title = <HipTitle title={"Payments - With related Order Info "} />;
  if (runningReport) {
    return (
      <>
        <button onClick={() => fetchPayments()}>Run Report</button>
        <HipMainWrapper>
          {Title}
          <HipBody>
            <HipLoadingTable />
          </HipBody>
        </HipMainWrapper>
      </>
    );
  }
  if (payments === undefined) {
    return (
      <>
        <button onClick={() => fetchPayments()}>Run Report</button>
      </>
    );
  } else {
    return (
      <>
        <button onClick={() => fetchPayments()}>Run Report</button>
        <HipMainWrapper>
          {Title}
          <HipBody>
            <HipErrorBoundary
              fallback={
                <HipClientError
                  type="alert"
                  thereWasAnErrorLoadingThe="Payments Table"
                  contactLink={true}
                />
              }
            >
              <PaymentsTable payments={payments} cluster={cluster} />
            </HipErrorBoundary>
          </HipBody>
        </HipMainWrapper>
      </>
    );
  }
};
