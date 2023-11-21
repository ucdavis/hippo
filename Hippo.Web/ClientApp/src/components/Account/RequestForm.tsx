import { useContext, useEffect, useState } from "react";
import "react-bootstrap-typeahead/css/Typeahead.css";
import { Redirect, useHistory, useParams } from "react-router-dom";
import AppContext from "../../Shared/AppContext";
import {
  GroupModel,
  IRouteParams,
  AccountCreateModel,
  RequestModel,
} from "../../types";
import { authenticatedFetch } from "../../util/api";
import { usePromiseNotification } from "../../util/Notifications";
import { GroupLookup } from "../Group/GroupLookup";

export const RequestForm = () => {
  const [context, setContext] = useContext(AppContext);
  const [notification, setNotification] = usePromiseNotification();

  const [groups, setGroups] = useState<GroupModel[]>([]);
  const [request, setRequest] = useState<AccountCreateModel>({
    groupId: 0,
    sshKey: "",
    supervisingPI: "",
  });

  const history = useHistory();
  const { cluster } = useParams<IRouteParams>();

  // load up possible groups
  useEffect(() => {
    const fetchGroups = async () => {
      const response = await authenticatedFetch(`/api/${cluster}/group/groups`);

      const groupsResult = await response.json();

      if (response.ok) {
        setGroups(groupsResult);
      }
    };

    fetchGroups();
  }, [cluster]);

  const handleSubmit = async () => {
    const req = authenticatedFetch(`/api/${cluster}/account/create`, {
      method: "POST",
      body: JSON.stringify(request),
    });

    setNotification(
      req,
      "Saving",
      "Request Created. Please wait for your sponsor to approve your request.",
      async (r) => {
        if (r.status === 400) {
          const errorText = await response.text(); //Bad Request Text
          return errorText;
        } else {
          return "An error happened, please try again.";
        }
      }
    );

    const response = await req;

    if (response.ok) {
      const request = (await response.json()) as RequestModel;

      setContext((ctx) => ({
        ...ctx,
        openRequests: [...ctx.openRequests, { ...request }],
      }));
      history.replace(`/${cluster}/pendingapproval`);
    }
  };

  if (
    context.openRequests.find(
      (r) => r.cluster === cluster && r.action === "CreateAccount"
    )
  ) {
    // there's already a request for this cluster, redirect to pending page
    return <Redirect to={`/${cluster}/pendingapproval`} />;
  }

  return (
    <div className="row justify-content-center">
      <div className="col-md-8">
        <h3>
          Welcome,{" "}
          <span className="status-color">{context.user.detail.firstName}</span>
        </h3>
        <p>
          You don't seem to have an account on this cluster yet. If you'd like
          access, please answer the&nbsp;questions&nbsp;below
        </p>
        <hr />
        <div className="form-group">
          <label>Who is sponsoring your account?</label>
          <GroupLookup
            setSelection={(group) =>
              setRequest((r) => ({ ...r, groupId: group.id }))
            }
            options={groups}
          />
          <p className="form-helper">
            Your group is probably named after your PI or your Department. You
            can filter this list by typing in it.
            <br />
            If you don't see your group, you may contact HPC help to request it
            be added.{" "}
            <a
              href={`mailto: hpc-help@ucdavis.edu?subject=Please add a group to the ${cluster} cluster&body=Group Name:  %0D%0API or Dept Email: `}
            >
              Click here to contact HPC Help
            </a>
          </p>
        </div>
        <div className="form-group">
          <label className="form-label">Who is your supervising PI?</label>
          <input
            className="form-control"
            id="supervisingPI"
            placeholder="Supervising PI"
            value={request.supervisingPI}
            onChange={(e) =>
              setRequest((r) => ({ ...r, supervisingPI: e.target.value }))
            }
          ></input>
          <p className="form-helper">
            Some clusters may require additional clarification on who your
            supervising PI will be. If you are unsure, please ask your sponsor.
          </p>
        </div>
        <div className="form-group">
          <label className="form-label">What is your Public SSH key</label>
          <textarea
            className="form-control"
            id="sharedKey"
            placeholder="Paste your public SSH key here"
            required
            onChange={(e) => {
              const value = e.target.value
                .replaceAll("\r", "")
                .replaceAll("\n", "");
              e.target.value = value;
              setRequest((r) => ({ ...r, sshKey: value.trim() }));
            }}
          ></textarea>
          <p className="form-helper">
            Paste all of the text from your public SSH file here. Example:
            <br></br>
            <code>
              ssh-rsa AAAAB3NzaC1yc....NrRFi9wrf+M7Q== fake@addr.local
            </code>
          </p>
        </div>
        <button
          disabled={notification.pending}
          onClick={handleSubmit}
          className="btn btn-primary"
        >
          Submit
        </button>
        <div>
          <br />
          <br />
          <p className="form-helper">
            To generate a ssh key pair please see this link:{" "}
            <a href="https://hpc.ucdavis.edu/faq#ssh-key" target={"blank"}>
              https://hpc.ucdavis.edu/faq#ssh-key
            </a>
          </p>
          <p className="form-helper">
            You will find instructions here for Windows, OS X, and Linux
            environments. Windows users: an openssh or SSH2 formatted ssh public
            key is required. See screenshots.
          </p>
          <p className="form-helper">
            (If your public key resides in the default location for OS X and
            Linux, ~/.ssh/, right click in the Name column of the window that
            will open when you click "Choose File" above. Select "Show Hidden
            Files".)
          </p>
        </div>
      </div>
    </div>
  );
};
