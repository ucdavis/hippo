import React, { useState } from "react";
import { useHistory } from "react-router-dom";

import { Account } from "../types";
import { authenticatedFetch } from "../util/api";
import { usePromiseNotification } from "../util/Notifications";
import { useConfirmationDialog } from "./ConfirmationDialog";
import { notEmptyOrFalsey } from "../util/ValueChecks";

interface Props {
  account: Account;
  removeAccount: (account: Account) => void;
  updateUrl: string;
}

export const RejectRequest = (props: Props) => {
  //const history = useHistory();
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
    [reason, setReason, notification.pending]
  );

  const reject = async () => {
    const [confirmed, reason] = await getConfirmation();
    if (!confirmed) {
      return;
    }

    const request = authenticatedFetch(
      `${props.updateUrl}${props.account.id}`,
      {
        method: "POST",
        body: JSON.stringify({ reason }),
      }
    );

    setNotification(request, "Saving", "Request Rejection Saved");

    const response = await request;

    if (response.ok) {
      //history.replace(`/project/details/${props.project.id}`);
      props.removeAccount(props.account);
    }
  };
  return (
    <button onClick={reject} className="btn btn-danger">
      Reject
    </button>
  );
};
