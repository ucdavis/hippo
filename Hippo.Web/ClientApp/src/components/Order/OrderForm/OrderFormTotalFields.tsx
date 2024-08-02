import { OrderModel } from "../../../types";
import { useFormContext, useWatch } from "react-hook-form";
import { faDollarSign } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import OrderFormField from "./OrderFormField";
import { Row } from "reactstrap";

type OrderFormTotalFieldsProps = {};

const OrderFormTotalFields: React.FC<OrderFormTotalFieldsProps> = ({}) => {
  const { setValue } = useFormContext<OrderModel>();

  // watch these values and recalculate when they change
  // to have changes trigger validation for the total field,
  // add it as a dep prop for quanitity, unitPrice, and adjustment fields
  const quantity: number = useWatch({
    name: "quantity",
  });

  const unitPrice = useWatch({
    name: "unitPrice",
  });

  const adjustment: number = useWatch({
    name: "adjustment",
  });

  const subTotal = quantity * parseFloat(unitPrice);
  const total = subTotal + Number(adjustment);

  setValue("subTotal", subTotal.toFixed(2));
  setValue("total", total.toFixed(2));

  return (
    <>
      <Row>
        <OrderFormField
          name="subTotal"
          label="SubTotal"
          canEditConditions={false} // can never edit
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
          type="number"
        />
        <OrderFormField
          type="number"
          name="total"
          label="Total"
          min={0.01}
          canEditConditions={false} // can never edit
          inputPrepend={<FontAwesomeIcon icon={faDollarSign} />}
        />
      </Row>
    </>
  );
};

export default OrderFormTotalFields;
