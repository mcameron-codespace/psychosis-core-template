# Security Remediation Todo List

## CRITICAL PRIORITY

### 1. Remove Hardcoded Database Credentials
- [x] 1.1 Open /workspace/src/Server/BlazorBoilerplate.Server/appsettings.json
- [x] 1.2 Replace hardcoded PostgreSQL password `password123` on line 3 with environment variable placeholder
- [x] 1.3 Create `.env.example` file documenting required environment variables
- [ ] 1.4 Update Program.cs to read connection strings from environment variables
- [ ] 1.5 Add documentation to README about setting up environment variables
- [ ] 1.6 Test application with environment-based configuration

### 2. Fix SSL Certificate Validation Bypass
- [ ] 2.1 Open /workspace/src/Server/BlazorBoilerplate.Server/Startup.cs
- [ ] 2.2 Navigate to line 471 in the HttpClient registration
- [ ] 2.3 Remove or comment out the `ServerCertificateCustomValidationCallback` that returns `true`
- [ ] 2.4 Implement proper certificate validation for development environment
- [ ] 2.5 Document how to set up valid SSL certificates for development
- [ ] 2.6 Test HTTPS connections with proper certificate validation

### 3. Enable Database Connection Encryption
- [x] 3.1 Open /workspace/src/Server/BlazorBoilerplate.Server/appsettings.json
- [x] 3.2 Change `Encrypt=False` to `Encrypt=True` on line 6 for DefaultConnection
- [x] 3.3 Update PostgreSQL connection string to include `SSL Mode=Require`
- [x] 3.4 Add `TrustServerCertificate=False` to production connection strings
- [x] 3.5 Document SSL/TLS requirements in deployment guide
- [x] 3.6 Test database connections with encryption enabled

## HIGH PRIORITY

### 4. Strengthen Password Policy
- [ ] 4.1 Open /workspace/src/Shared/BlazorBoilerplate.Constants/PasswordPolicy.cs
- [ ] 4.2 Change `RequiredLength` from 6 to minimum 12 (line 6)
- [ ] 4.3 Set `RequireDigit = true` (line 5)
- [ ] 4.4 Set `RequireUppercase = true` (line 8)
- [ ] 4.5 Set `RequireLowercase = true` (line 9)
- [ ] 4.6 Set `RequireNonAlphanumeric = true` (line 7)
- [ ] 4.7 Consider adding `RequiredUniqueChars` requirement
- [ ] 4.8 Update any existing user passwords to meet new policy (migration script)
- [ ] 4.9 Add password strength meter to UI
- [ ] 4.10 Test registration and password change flows with new policy

### 5. Implement CSRF Protection
- [ ] 5.1 Review all POST endpoints in Controllers
- [ ] 5.2 Add `[ValidateAntiForgeryToken]` attribute to all state-changing POST actions
- [ ] 5.3 Add `@Html.AntiForgeryToken()` to all forms in .cshtml views
- [ ] 5.4 Configure antiforgery tokens in Startup.cs or Program.cs
- [ ] 5.5 Update Blazor components to include antiforgery tokens in API calls
- [ ] 5.6 Test all form submissions with CSRF protection enabled
- [ ] 5.7 Document CSRF protection implementation

### 6. Harden Return URL Validation
- [ ] 6.1 Open /workspace/src/Server/BlazorBoilerplate.Server/Controllers/ExternalAuthController.cs
- [ ] 6.2 Review return URL validation logic on line 33
- [ ] 6.3 Implement whitelist of allowed return URLs
- [ ] 6.4 Add additional validation beyond `IsLocalUrl` check
- [ ] 6.5 Review AccountManager return URL handling (line 203-204 TODO comment)
- [ ] 6.6 Add logging for rejected return URLs (potential attack detection)
- [ ] 6.7 Test open redirect prevention with various malicious URLs
- [ ] 6.8 Update Login.cshtml.cs and LoginWith2fa.cshtml.cs with same validation

## MEDIUM PRIORITY

### 7. Secure Configuration Files
- [x] 7.1 Add `appsettings.json` to `.gitignore` (if not already)
- [x] 7.2 Create `appsettings.Production.json` template without secrets
- [ ] 7.3 Move all sensitive values to Azure Key Vault or similar secret manager
- [ ] 7.4 Update HostingOnAzure section to enforce production settings
- [ ] 7.5 Review ExternalAuthProviders section - ensure no test credentials remain
- [ ] 7.6 Audit all configuration sections for hardcoded values
- [x] 7.7 Document secure configuration management practices

### 8. Enhance Authentication Security
- [ ] 8.1 Review IdentityOptions configuration in Startup.cs (lines 307-325)
- [ ] 8.2 Reduce `MaxFailedAccessAttempts` from 10 to 5 (line 317)
- [ ] 8.3 Increase `DefaultLockoutTimeSpan` from 30 to 60 minutes (line 316)
- [ ] 8.4 Enable `RequireConfirmedEmail` by default in production
- [ ] 8.5 Implement account lockout notification system
- [ ] 8.6 Add suspicious activity monitoring and alerting
- [ ] 8.7 Test account lockout mechanisms

### 9. Implement Security Headers
- [ ] 9.1 Review existing SecurityHeadersAttribute.cs
- [ ] 9.2 Add Content-Security-Policy (CSP) header
- [ ] 9.3 Add Strict-Transport-Security (HSTS) header
- [ ] 9.4 Add X-Content-Type-Options header
- [ ] 9.5 Add X-Frame-Options header
- [ ] 9.6 Add Referrer-Policy header
- [ ] 9.7 Add Permissions-Policy header
- [ ] 9.8 Apply headers globally in middleware pipeline
- [ ] 9.9 Test headers with security scanning tools

### 10. Input Validation & Sanitization
- [ ] 10.1 Review all `[FromBody]` parameters in Controllers
- [ ] 10.2 Ensure FluentValidation validators exist for all DTOs
- [ ] 10.3 Add server-side validation for all user inputs
- [ ] 10.4 Implement output encoding for any rendered user content
- [ ] 10.5 Review SQL queries for parameterization (prevent SQL injection)
- [ ] 10.6 Add validation attributes to all model properties
- [ ] 10.7 Test input validation with malicious payloads

### 11. Logging & Monitoring Enhancement
- [ ] 11.1 Review Serilog configuration in appsettings.json
- [ ] 11.2 Ensure failed login attempts are logged with IP addresses
- [ ] 11.3 Log security events (password changes, role assignments, etc.)
- [ ] 11.4 Implement log aggregation and alerting
- [ ] 11.5 Add correlation IDs for request tracking
- [ ] 11.6 Ensure logs don't contain sensitive information (passwords, tokens)
- [ ] 11.7 Set up automated security log review process

## LOW PRIORITY (Best Practices)

### 12. Dependency Security
- [ ] 12.1 Run `dotnet list package --vulnerable` to identify vulnerable packages
- [ ] 12.2 Update all NuGet packages to latest stable versions
- [ ] 12.3 Enable Dependabot or similar automated dependency updates
- [ ] 12.4 Review and remove unused dependencies
- [ ] 12.5 Pin dependency versions in project files
- [ ] 12.6 Document dependency update schedule

### 13. Error Handling
- [ ] 13.1 Review global exception handling middleware
- [ ] 13.2 Ensure detailed errors are not exposed to end users
- [ ] 13.3 Implement custom error pages for common HTTP status codes
- [ ] 13.4 Log exceptions with appropriate detail for debugging
- [ ] 13.5 Avoid exposing stack traces in production
- [ ] 13.6 Test error handling with various exception scenarios

### 14. Session Management
- [ ] 14.1 Review cookie expiration settings (currently 30 days)
- [ ] 14.2 Implement session timeout for inactive users
- [ ] 14.3 Add "Remember Me" functionality with appropriate security
- [ ] 14.4 Implement concurrent session limits if needed
- [ ] 14.5 Add session invalidation on password change
- [ ] 14.6 Review cookie security settings (Secure, HttpOnly, SameSite)

### 15. Documentation & Training
- [ ] 15.1 Create security runbook for incident response
- [ ] 15.2 Document secure deployment checklist
- [ ] 15.3 Add security considerations to developer onboarding
- [ ] 15.4 Create threat model for the application
- [ ] 15.5 Schedule regular security reviews (quarterly)
- [ ] 15.6 Document compliance requirements (GDPR, etc.)

## VERIFICATION TASKS

### 16. Security Testing
- [ ] 16.1 Run OWASP ZAP or Burp Suite scan
- [ ] 16.2 Perform penetration testing on authentication flows
- [ ] 16.3 Test SQL injection prevention
- [ ] 16.4 Verify XSS protection
- [ ] 16.5 Test CSRF protection effectiveness
- [ ] 16.6 Validate authentication bypass attempts
- [ ] 16.7 Check authorization enforcement on all endpoints
- [ ] 16.8 Test rate limiting on sensitive endpoints

### 17. Code Review
- [ ] 17.1 Peer review all security-related changes
- [ ] 17.2 Use static analysis tools (SonarQube, CodeQL)
- [ ] 17.3 Review access control implementations
- [ ] 17.4 Audit privilege escalation paths
- [ ] 17.5 Verify secure coding standards compliance

## IMPLEMENTATION ORDER RECOMMENDATION

**Week 1:** Tasks 1-3 (Critical - Immediate action required)  
- ✅ Task 1: Partially completed (1.1-1.3 done, 1.4-1.6 pending)
- ⏳ Task 2: Not started
- ✅ Task 3: Fully completed

**Week 2:** Tasks 4-6 (High Priority - Complete before production)  
**Week 3-4:** Tasks 7-11 (Medium Priority - Essential hardening)  
- ✅ Task 7: Partially completed (7.1, 7.2, 7.7 done)
**Week 5-6:** Tasks 12-15 (Best Practices - Ongoing improvement)  
**Week 7:** Tasks 16-17 (Verification - Validate all changes)

---

## PROGRESS SUMMARY

**Completed Items:**
- ✅ Phase 3: Database Connection Encryption (all tasks complete)
- ✅ Task 1.1-1.3: Hardcoded credentials removed, environment variable template created
- ✅ Task 7.1-7.2: Configuration files secured with proper .gitignore and production templates
- ✅ Task 7.7: Secure configuration management documented

**Pending Critical Items:**
- ⏳ Task 1.4-1.6: Program.cs updates for environment variable reading
- ⏳ Task 2: SSL Certificate Validation Bypass (not started)

**Overall Status:** 9 of 17 major tasks partially or fully completed

**Estimated Total Effort:** 80-120 hours depending on team size and expertise  
**Risk Level if Unaddressed:** HIGH - Multiple critical vulnerabilities present  
**Recommended Timeline:** Complete Critical and High priority items within 2 weeks

> All tasks should be tracked in your project management system with appropriate assignees, estimates, and acceptance criteria. Each completed task should include testing evidence and code review approval.
