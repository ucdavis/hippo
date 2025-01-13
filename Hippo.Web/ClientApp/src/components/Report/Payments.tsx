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
import ReportFilter from "./ReportFilter";

export const Payments = () => {
  const [payments, setPayments] = useState<PaymentReportModel[]>();
  const { cluster } = useParams();
  const [runningReport, setRunningReport] = useState(false);
  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");
  const [filterType, setFilterType] = useState<string>("PaymentDate");

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
      `/api/${cluster}/report/payments?filterType=${filterType}&start=${startDate}&end=${endDate}`,
    );

    if (response.ok) {
      const data = await response.json();
      setPayments(data);
    } else {
      alert("Error fetching payments");
    }
    setRunningReport(false);
  };

  const Title = (
    <HipTitle
      title={"Payments - With related Order Info "}
      subtitle="This will only show payments that have completed in sloth."
    />
  );
  if (runningReport) {
    return (
      <>
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
      <ReportFilter
        runReport={fetchPayments}
        startDate={startDate}
        setStartDate={setStartDate}
        endDate={endDate}
        setEndDate={setEndDate}
        filterType={filterType}
        setFilterType={setFilterType}
      />
    );
  } else {
    return (
      <>
        <ReportFilter
          runReport={fetchPayments}
          startDate={startDate}
          setStartDate={setStartDate}
          endDate={endDate}
          setEndDate={setEndDate}
          filterType={filterType}
          setFilterType={setFilterType}
        />
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
