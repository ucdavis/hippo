import React from "react";
import { useFormContext } from "react-hook-form";
import HipFormField from "../../../Shared/Form/HipFormField";
import { HipFormFieldProps } from "../../../Shared/Form/formTypes";
import { OrderModel } from "../../../types";

type OrderFormFieldProps = HipFormFieldProps<OrderModel> & {};

const OrderFormField: React.FC<OrderFormFieldProps> = ({ ...props }) => {
  const {
    register,
    formState: { errors },
  } = useFormContext<OrderModel>();

  return (
    <HipFormField register={register} error={errors[props.name]} {...props} />
  );
};

export default OrderFormField;
