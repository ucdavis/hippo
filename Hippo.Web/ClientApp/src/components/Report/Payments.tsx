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

  useEffect(() => {
    setPayments(undefined);
  }, []);

  useEffect(() => {
    const fetchPayments = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/report/payments`,
      );

      if (response.ok) {
        const data = await response.json();
        setPayments(data);
      } else {
        alert("Error fetching payments");
      }
    };

    fetchPayments();
  }, [cluster]);

  // RH TODO: handle loading/error states
  const Title = <HipTitle title={"Payments - With related Order Info "} />;
  if (payments === undefined) {
    return (
      <HipMainWrapper>
        {Title}
        <HipBody>
          <HipLoadingTable />
        </HipBody>
      </HipMainWrapper>
    );
  } else {
    return (
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
    );
  }
};
