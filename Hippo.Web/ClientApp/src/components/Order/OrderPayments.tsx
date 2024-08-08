import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import { PaymentTable } from "./PaymentTable";

export const OrderPayments: React.FC = () => {
  // RH TODO: use order name ?
  return (
    <HipMainWrapper>
      <HipTitle title="" subtitle="Details" />
      <HipBody>
        <PaymentTable
          numberOfRows={1000}
          showLinkToAll={false}
          paymentCount={0}
        />
      </HipBody>
    </HipMainWrapper>
  );
};
