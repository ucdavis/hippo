import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";
import { PaymentTable } from "./PaymentTable";

export const OrderPayments: React.FC = () => {
  // RH TODO: use order name ?
  return (
    <HipMainWrapper>
      <HipTitle title="" subtitle="Details" />
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
          <PaymentTable
            numberOfRows={1000}
            showLinkToAll={false}
            paymentCount={0}
          />
        </HipErrorBoundary>
      </HipBody>
    </HipMainWrapper>
  );
};
