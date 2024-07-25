import React from "react";
import { useFormContext } from "react-hook-form";
import FormField from "../../../Shared/Form/FormField";
import { FormFieldProps } from "../../../Shared/Form/formTypes";
import { OrderModel } from "../../../types";

type OrderFormFieldProps = FormFieldProps<OrderModel> & {};

const OrderFormField: React.FC<OrderFormFieldProps> = ({ ...props }) => {
  const {
    register,
    formState: { errors },
  } = useFormContext<OrderModel>();

  return (
    <FormField register={register} error={errors[props.name]} {...props} />
  );
};

export default OrderFormField;
