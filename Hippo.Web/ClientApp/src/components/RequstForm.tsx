import { useContext, useEffect, useState } from "react";

import AppContext from "../Shared/AppContext";
import { Account, RequestPostModel } from "../types";
import { authenticatedFetch } from "../util/api";

export const RequestForm = () => {
  const user = useContext(AppContext).user;

  const [sponsors, setSponsors] = useState<Account[]>([]);
  const [request, setRequest] = useState<RequestPostModel>({
    sponsorId: 0,
    sshKey: "",
  });

  // load up possible sponsors
  useEffect(() => {
    const fetchSponsors = async () => {
      const response = await authenticatedFetch("/api/account/sponsors");

      if (response.ok) {
        setSponsors(await response.json());
      }
    };

    fetchSponsors();
  }, []);

  const handleSubmit = async () => {
    const response = await authenticatedFetch("/api/account/create", {
      method: "POST",
      body: JSON.stringify(request),
    });

    if (response.ok) {
      const newAccount = await response.json();

      console.log("new acct", newAccount);
      // do something with it
    }
  };

  return (
    <div className="row justify-content-center">
      <div className="col-md-6">
        <h3>
          Welcome, <span className="status-color">{user.detail.firstName}</span>
        </h3>
        <p>
          You don't seem to have an account on Farm yet. If you’d like access,
          please answer the questions below
        </p>
        <hr />
        <div className="form-group">
          <label>Who is sponsoring your account?</label>
          <select
            onChange={(e) =>
              setRequest({ ...request, sponsorId: Number(e.target.value) })
            }
            className="form-select"
            aria-label="Default select example"
          >
            {sponsors.map((sponsor) => (
              <option key={sponsor.id} value={sponsor.id}>
                {sponsor.name}
              </option>
            ))}
          </select>
          <p className="form-helper">Help text</p>
        </div>
        <div className="form-group">
          <label className="form-label">What is your Public SSH key</label>
          <textarea
            className="form-control"
            id="exampleFormControlTextarea1"
            onChange={(e) => setRequest({ ...request, sshKey: e.target.value })}
          ></textarea>
          <p className="form-helper">
            Paste all of the text from your public SSH file here.
          </p>
        </div>
        <button onClick={handleSubmit} className="btn btn-primary">
          Submit
        </button>
      </div>
    </div>
  );
};
