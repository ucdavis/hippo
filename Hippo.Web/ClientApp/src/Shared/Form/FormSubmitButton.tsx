import React from "react";
import { Button } from "reactstrap";
import { OrderModel } from "../../types";
import { useFormContext } from "react-hook-form";

interface FormSubmitButtonProps {}

const FormSubmitButton: React.FC<FormSubmitButtonProps> = ({}) => {
  const {
    formState: { isDirty, isSubmitting, isSubmitSuccessful, errors },
  } = useFormContext<OrderModel>();

  const shouldDisable = !isDirty || isSubmitting || isSubmitSuccessful;

  return (
    <>
      <Button
        type="submit"
        block={true}
        outline={!shouldDisable} // for styling
        color="primary"
        disabled={shouldDisable}
      >
        {isSubmitting
          ? "Submitting..."
          : isSubmitSuccessful
            ? "Submitted!"
            : "Submit"}
      </Button>
      {errors.root && (
        <span className="text-danger">{errors.root?.message}</span>
      )}
    </>
  );
};

export default FormSubmitButton;
