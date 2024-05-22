import React from "react";
import { Button } from "reactstrap";

interface FormSubmitButtonProps {
  isSubmitting: boolean;
  isSubmitSuccessful: boolean;
}

const FormSubmitButton: React.FC<FormSubmitButtonProps> = ({
  isSubmitting,
  isSubmitSuccessful,
}) => {
  return (
    <Button
      type="submit"
      block={true}
      outline={!isSubmitSuccessful}
      color="primary"
      disabled={isSubmitSuccessful}
    >
      {isSubmitting
        ? "Submitting..."
        : isSubmitSuccessful
          ? "Submitted!"
          : "Submit"}
    </Button>
  );
};

export default FormSubmitButton;
