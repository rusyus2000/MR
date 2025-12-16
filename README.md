# My Project

## Export CSV Format

The admin export endpoint `POST /api/items/export` returns a CSV with all required fields first, followed by optional metadata (existing), then the newly added optional fields. Order and exact header labels are as follows:

1. Id
2. Title
3. Description
4. Url
5. Asset Type
6. Domain
7. Division
8. Service Line
9. Data Source
10. Status
11. Owner Name
12. Owner Email
13. Executive Sponsor Name
14. Executive Sponsor Email
15. Operating Entity
16. Refresh Frequency
17. PHI
18. PII
19. Has RLS
20. Last Modified Date (yyyy-MM-dd)
21. Date Added (yyyy-MM-dd)
22. Featured
23. Tags (semicolon-separated)
24. Data Consumers (free text)
25. Dependencies
26. Default AD Group Names
27. Product Group
28. Product Status Notes
29. Tech Delivery Mgr
31. Regulatory/Compliance/Contractual
32. BI Platform
33. DB Server
34. DB/Data Mart
35. Database Table
36. Source Rep
37. dataSecurityClassification
38. accessGroupName
39. accessGroupDN
40. Automation Classification
41. user_visibility_string
42. user_visibility_number
43. Epic Security Group tag
44. Keep Long Term

Notes:
- Optional text fields are empty when values are not set.
- Boolean fields (PHI, PII, Has RLS, Featured) are nullable; when missing they export as `Missing Data`, otherwise `true`/`false`.
- Tags are semicolon-separated; Data Consumers is a single free-text field.
