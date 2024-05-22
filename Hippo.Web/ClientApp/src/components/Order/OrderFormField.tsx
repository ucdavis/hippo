import React from "react";
import { OrderModel } from "../../types";
import FormField, { FormFieldProps } from "../../Shared/Form/FormField";
import { useFormContext } from "react-hook-form";

type OrderFormFieldProps = FormFieldProps<OrderModel>;

const OrderFormField: React.FC<OrderFormFieldProps> = ({ ...deferred }) => {
  const {
    register,
    formState: { errors },
  } = useFormContext<OrderModel>();

  return <FormField register={register} errors={errors} {...deferred} />;
};

export default OrderFormField;
