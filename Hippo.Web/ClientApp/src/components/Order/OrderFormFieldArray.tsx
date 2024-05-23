import React from "react";
import { ArrayPath, useFormContext } from "react-hook-form";
import { OrderModel } from "../../types";

import FormFieldArray from "../../Shared/Form/FormFieldArray";

type OrderFormFieldArrayProps = {
  name: ArrayPath<OrderModel>;
};

const OrderFormFieldArray: React.FC<OrderFormFieldArrayProps> = ({ name }) => {
  const { control } = useFormContext<OrderModel>();

  return (
    <>
      <FormFieldArray control={control} name={name} />
    </>
  );
};

export default OrderFormFieldArray;
