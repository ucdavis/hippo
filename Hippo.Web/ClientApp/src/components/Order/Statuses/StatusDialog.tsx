import React from "react";
import { UpdateOrderStatusModel } from "./status";
import StatusBar from "./StatusBar";
import StatusCard from "./StatusCard";

interface StatusDialogProps extends UpdateOrderStatusModel {
  isAdmin: boolean;
  hideDescription?: boolean;
  children?: React.ReactNode;
  newStatusDanger?: boolean;
}

const StatusDialog: React.FC<StatusDialogProps> = ({
  newStatus,
  currentStatus,
  hideDescription = false,
  isAdmin,
  newStatusDanger = false,
  children,
}) => {
  return (
    <div>
      <StatusBar
        status={currentStatus}
        showOnHover={newStatus}
        hideTooltip={true}
      />
      <h3>
        You are changing the status of this order from{" "}
        <span className="hip-text-primary">{currentStatus}</span> to{" "}
        <span
          className={
            newStatusDanger ? "hip-text-danger-dark" : "hip-text-primary"
          }
        >
          {newStatus}
        </span>
        .
      </h3>
      {!hideDescription && (
        <>
          <br />
          <StatusCard
            status={currentStatus}
            isAdmin={isAdmin}
            showStatusActions={false}
          />
          <StatusCard
            status={newStatus}
            isAdmin={isAdmin}
            showStatusActions={true}
          />
        </>
      )}
      {children}
    </div>
  );
};

export default StatusDialog;
