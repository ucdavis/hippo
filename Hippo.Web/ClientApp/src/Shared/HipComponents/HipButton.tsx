import React from "react";
import { Button, ButtonProps } from "reactstrap";

interface HipButtonProps extends ButtonProps {}

const HipButton: React.FC<HipButtonProps> = ({
  disabled = false,
  type = "button",
  color = "primary",
  children,
  ...props
}) => {
  return (
    <Button disabled={disabled} color={color} {...props}>
      {children}
    </Button>
  );
};

export default HipButton;
