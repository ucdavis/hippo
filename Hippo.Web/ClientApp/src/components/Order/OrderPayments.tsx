import { PaymentTable } from "./PaymentTable";

export const OrderPayments: React.FC = () => {
  return (
    <PaymentTable numberOfRows={1000} showLinkToAll={false} paymentCount={0} />
  );
};
