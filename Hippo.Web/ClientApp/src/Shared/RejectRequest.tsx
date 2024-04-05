import React, { useState } from "react";

import { RequestModel } from "../types";
import { authenticatedFetch, parseBadRequest } from "../util/api";
import { usePromiseNotification } from "../util/Notifications";
import { useConfirmationDialog } from "./ConfirmationDialog";
import { notEmptyOrFalsey } from "../util/ValueChecks";

interface Props {
  request: RequestModel;
  removeAccount: (request: RequestModel) => void;
  updateUrl: string;
  disabled?: boolean;
}

export const RejectRequest = (props: Props) => {
  const [reason, setReason] = useState("");

  const [notification, setNotification] = usePromiseNotification();

  const [getConfirmation] = useConfirmationDialog<string>(
    {
      title: "Reject Request",
      message: (setReturn) => (
        <div className="form-group">
          <label htmlFor="fieldName">Reason</label>
          <textarea
            className="form-control"
            id="fieldName"
            rows={3}
            required
            value={reason}
            onChange={(e) => {
              setReason(e.target.value);
              setReturn(e.target.value);
            }}
          />
          <small id="fieldNameHelp" className="form-text text-muted">
            Let us know what issues you have with this request.
          </small>
        </div>
      ),
      canConfirm: notEmptyOrFalsey(reason) && !notification.pending,
    },
    [reason, setReason, notification.pending],
  );

  const reject = async () => {
    const [confirmed, reason] = await getConfirmation();
    if (!confirmed) {
      return;
    }

    const request = authenticatedFetch(
      `${props.updateUrl}${props.request.id}`,
      {
        method: "POST",
        body: JSON.stringify({ reason }),
      },
    );

    setNotification(request, "Saving", "Request Rejection Saved", async (r) => {
      if (r.status === 400) {
        const errors = await parseBadRequest(response);
        return errors;
      } else {
        return "An error happened, please try again.";
      }
    });

    const response = await request;

    if (response.ok) {
      props.removeAccount(props.request);
    }
  };
  return (
    <button
      id="rejectButton"
      disabled={props.disabled === true}
      onClick={reject}
      className="btn btn-danger"
    >
      Reject
    </button>
  );
};
