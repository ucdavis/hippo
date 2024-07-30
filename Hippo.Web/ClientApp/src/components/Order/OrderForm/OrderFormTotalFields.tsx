import { OrderModel } from "../../../types";
import { useFormContext, useWatch } from "react-hook-form";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import OrderFormField from "./OrderFormField";

type OrderFormTotalFieldsProps = {};

const OrderFormTotalFields: React.FC<OrderFormTotalFieldsProps> = () => {
  const { setValue } = useFormContext<OrderModel>();

  // watch these values and recalculate when they change
  // to have changes trigger validation for the total field,
  // add it as a dep prop for quanitity, unitPrice, and adjustment fields
  const quantity = useWatch({
    name: "quantity",
  });

  const unitPrice = useWatch({
    name: "unitPrice",
  });

  const adjustment = useWatch({
    name: "adjustment",
  });

  const subTotal = quantity * parseFloat(unitPrice);
  const total = subTotal + Number(adjustment);

  setValue("subTotal", subTotal.toFixed(2));
  setValue("total", total.toFixed(2));

  return (
    <>
      <OrderFormField
        name="subTotal"
        label="SubTotal"
        readOnly={true}
        disabled={true}
        inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
        type="number"
      />
      <OrderFormField
        type="number"
        name="total"
        label="Total"
        min={0.01}
        readOnly={true}
        disabled={true}
        inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
      />
    </>
  );
};

export default OrderFormTotalFields;
