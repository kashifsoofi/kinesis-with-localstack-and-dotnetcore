FROM coredns/coredns

COPY Corefile .

ENTRYPOINT ["./coredns", "-conf", "Corefile"]